from collections import deque
import io
import random
import socket
import sys
import numpy as np
import os
import tensorflow as tf

from PIL import Image

# Function to load the model
def load_saved_model(model_path):
    model = tf.keras.models.load_model(model_path)
    model.compile(optimizer=tf.keras.optimizers.Adam(learning_rate=0.001),
                  loss='categorical_crossentropy',
                  metrics=['accuracy'])
    return model

# Predict skill
def predict_skill(model, image, class_names):
    image = preprocess_image(image)

    prediction = model.predict(image)[0]
    predicted_skill_index = np.argmax(prediction)
    confidence = prediction[predicted_skill_index]
    predicted_skill_name = class_names[predicted_skill_index]
    return predicted_skill_name, confidence, prediction

class ExperienceReplay:
    def __init__(self, buffer_size=100, num_classes=8):
        self.buffers = [deque(maxlen=buffer_size) for _ in range(num_classes)]
        self.preloaded_samples = [deque(maxlen=buffer_size) for _ in range(num_classes)]

    def add_sample(self, image, skill_index):
        self.buffers[skill_index].append(image)

    def add_preloaded_sample(self, image, skill_index):
        self.preloaded_samples[skill_index].append(image)

    def get_batch(self, new_image, new_skill_index):
        batch_images = [new_image]
        batch_labels = [new_skill_index]

        for skill_index in range(len(self.buffers)):
            if skill_index == new_skill_index:
                continue

            user_images = [img for img in self.buffers[skill_index]]
            if user_images:
                sample = random.choice(user_images)
            elif self.preloaded_samples[skill_index]:
                sample = random.choice([img for img in self.preloaded_samples[skill_index]])
            else:
                continue
            batch_images.append(sample)
            batch_labels.append(skill_index)

        return batch_images, batch_labels

def load_initial_buffer(experience_replay, class_names, folder_name='BufferImages'):
    script_dir = os.path.dirname(os.path.abspath(__file__))
    base_path = os.path.join(script_dir, folder_name)
    for class_index, class_name in enumerate(class_names):
        print(f"Loading images for {class_name}")
        class_path = os.path.join(base_path, class_name)
        if os.path.exists(class_path):
            for file_name in os.listdir(class_path):
                if file_name.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp', '.gif')):
                    image_path = os.path.join(class_path, file_name)
                    with open(image_path, 'rb') as file:
                        byte_array = file.read()
                        image = Image.open(io.BytesIO(byte_array))
                    if image is not None:
                        experience_replay.add_preloaded_sample(image, class_index)
        else:
            print(f"Warning: Path not found for class {class_name} at {class_path}")


def preprocess_image(image: Image.Image):
    try:
        # Convert to grayscale
        image = image.convert('L')
        
        # Resize the image
        image = image.resize((128, 128))
        
        # Convert to numpy array
        image = np.array(image)
        
        # Reshape and normalize
        image = image.reshape(1, 128, 128, 1).astype('float32') / 255
        
        return image
    except Exception as e:
        print(f"Error preprocessing image: {e}")
        return None

def update_model(model, experience_replay, new_image, new_skill_index):
    batch_images, batch_labels = experience_replay.get_batch(new_image, new_skill_index)
    
    # Preprocess and reshape correctly

    batch_images = np.array([preprocess_image(img) for img in batch_images])
    batch_images = batch_images.reshape(8,128,128,1)
    # Convert labels to categorical
    batch_labels = tf.keras.utils.to_categorical(batch_labels, num_classes=8)
    
    # Train the model for one step
    history = model.fit(batch_images, batch_labels, batch_size=len(batch_images), epochs=1, verbose=1)
    
    return history.history['loss'][0]



# Main execution
if __name__ == "__main__":
    class_names = ['fireball', 'frostbeam', 'heal', 'meteor', 'others', 'shield', 'summon', 'teleport']

    experience_replay = ExperienceReplay(buffer_size=100, num_classes=8)

    profile_name:str = ""


    try:
        host, port = "127.0.0.1", 25001
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect((host, port))
        profile_name = sock.recv(10000).decode('utf-8')
        load_initial_buffer(experience_replay, class_names)
        loaded_model = load_saved_model(os.path.dirname(os.path.abspath(sys.argv[0])) + "/models/" + profile_name +".h5")
        sock.sendall("Model loaded".encode("UTF-8"))
    except Exception as e:
        print(f"Error connecting to socket or load data: {e}")
        sys.exit(1)

    while True:
        received_data = sock.recv(10000)
        if not received_data:
            continue
        else:
            try:
                drawn_image = Image.open(io.BytesIO(received_data))
                predicted_skill, confidence, prediction_array = predict_skill(loaded_model, drawn_image, class_names)
                prediction_array = prediction_array * 100
                print(f"Predicted skill: {predicted_skill}")
                print(f"Confidence: {confidence}")
                sock.sendall(str([f"{name:} {value:.2f}%" for name,value in zip(class_names ,prediction_array)]).encode("UTF-8"))# send to unity
                if predicted_skill != "others":
                    if confidence > 0.95:
                        experience_replay.add_sample(drawn_image, class_names.index(predicted_skill))
                    if 0.90 < confidence <= 0.95:
                        loss = update_model(loaded_model, experience_replay, drawn_image, class_names.index(predicted_skill))
                        loaded_model.save(os.path.dirname(os.path.abspath(sys.argv[0])) + "/models/" + profile_name +".h5")
            except Exception as e:
                try:
                    text_from_user = received_data.decode('utf-8')
                    print(text_from_user)
                    if text_from_user in class_names: #new label for image during the game 
                        loss = update_model(loaded_model, experience_replay, drawn_image, class_names.index(text_from_user))
                        loaded_model.save(os.path.dirname(os.path.abspath(sys.argv[0])) + "/models/" + profile_name +".h5")
                        sock.sendall("Model updated".encode("UTF-8"))
                    # else: #the game is loaded we should recieve profile name
                    #     profile_name = text_from_user
                    #     model_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
                    #     base_path = os.path.join(model_dir,"models", profile_name + ".h5")
                    #     print(base_path)
                    #     model_save_path = base_path  # Path to save the model
                    #     loaded_model = load_saved_model(model_save_path)
                    #     sock.sendall("Model loaded".encode("UTF-8"))

                except Exception as inner_e:
                    print(f"Error during prediction: {inner_e}")
                    sock.sendall(f"Error during prediction: {inner_e}".encode("UTF-8"))
