import random
import numpy as np
import os
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Conv2D, MaxPooling2D, GlobalAveragePooling2D, Dense, Dropout
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from sklearn.model_selection import train_test_split
from PIL import Image


# List available devices
from tensorflow.python.client import device_lib
print("Available devices:")
for device in device_lib.list_local_devices():
    print(f" - {device.device_type}: {device.physical_device_desc}")

# Define the model
def create_model(num_classes):
    model = Sequential([
        Conv2D(32, (3, 3), activation='relu', input_shape=(128, 128, 1)),
        MaxPooling2D((2, 2)),
        Conv2D(64, (3, 3), activation='relu'),
        MaxPooling2D((2, 2)),
        Conv2D(64, (3, 3), activation='relu'),
        MaxPooling2D((2, 2)),
        Conv2D(128, (3, 3), activation='relu'),
        GlobalAveragePooling2D(),
        Dense(128, activation='relu'),
        Dropout(0.5),
        Dense(64, activation='relu'),
        Dropout(0.2),
        Dense(num_classes, activation='softmax')
    ])
    model.compile(optimizer=Adam(lr=0.001),
                  loss='categorical_crossentropy',
                  metrics=['accuracy'])
    
    return model

# Load images and labels from directories
def load_data(base_path, num_samples=900):
    images = []
    labels = []
    class_names = []
    
    for class_index, class_name in enumerate(sorted(os.listdir(base_path))):
        class_path = os.path.join(base_path, class_name)
        if os.path.isdir(class_path):
            class_names.append(class_name)
            image_files = os.listdir(class_path)
            if len(image_files) > num_samples:
                image_files = random.sample(image_files, num_samples)
            for image_name in image_files:
                image_path = os.path.join(class_path, image_name)
                try:
                    image = Image.open(image_path).convert('L')  # Convert to grayscale
                    image = image.resize((128, 128))
                    image = np.array(image)
                    images.append(image)
                    labels.append(len(class_names) - 1)
                except Exception as e:
                    print(f"Error loading image {image_path}: {e}")
    
    return np.array(images), np.array(labels), class_names

# Prepare data for training
def prepare_data(images, labels, num_classes):
    labels_categorical = to_categorical(labels, num_classes=num_classes)
    X_train, X_val, y_train, y_val = train_test_split(images, labels_categorical, test_size=0.2, random_state=42)
    
    X_train = X_train.reshape(X_train.shape[0], 128, 128, 1).astype('float32') / 255
    X_val = X_val.reshape(X_val.shape[0], 128, 128, 1).astype('float32') / 255
    
    return X_train, X_val, y_train, y_val

# Train and save the model
def train_and_save_model(X_train, y_train, X_val, y_val, num_classes, model_save_path):
    with tf.device('/device:DML:0'):
        model = create_model(num_classes)

        # Data augmentation for training
        datagen = ImageDataGenerator(
            rotation_range=5,
            width_shift_range=0.1,
            height_shift_range=0.1,
            zoom_range=0.1,
            horizontal_flip=False,
            vertical_flip=False
        )

        batch_size = 32
        epochs = 40
    
        history = model.fit(
            datagen.flow(X_train, y_train, batch_size=batch_size),
            steps_per_epoch=len(X_train) // batch_size,
            epochs=epochs,
            validation_data=(X_val, y_val),
            verbose=1
        )

        model.save(model_save_path)
        print(f"Model saved to {model_save_path}")

    return model, history

# Main execution
if __name__ == "__main__":
    base_path = '.'  # Assuming the script is in the same directory as the skill folders
    model_save_path = 'spell_recognition_model.h5'  # Path to save the model

    # Load data
    images, labels, class_names = load_data(base_path)
    print(f"Number of classes: {len(class_names)}")
    print(f"Class names: {class_names}")
    print(f"Number of images: {len(images)}")
    print(f"Label range: {np.min(labels)} to {np.max(labels)}")

    # Prepare data
    num_classes = len(class_names)
    X_train, X_val, y_train, y_val = prepare_data(images, labels, num_classes)

    # Train and save model
    model, history = train_and_save_model(X_train, y_train, X_val, y_val, num_classes, model_save_path)

    # Print final accuracy
    print(f"Final training accuracy: {history.history['accuracy'][-1]:.4f}")
    print(f"Final validation accuracy: {history.history['val_accuracy'][-1]:.4f}")