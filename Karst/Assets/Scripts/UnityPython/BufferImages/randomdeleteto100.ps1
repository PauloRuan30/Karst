# Specify the general folder where subfolders containing .jpg files are located
$generalFolderPath = "C:\Users\madrat\Desktop\build2\WizardVR_Data\Scripts\UnityPython\BufferImages"

# Get all subfolders in the general folder
$subfolders = Get-ChildItem -Path $generalFolderPath -Directory

# Loop through each subfolder
foreach ($folder in $subfolders) {
    # Get all .jpg files in the current subfolder
    $jpgFiles = Get-ChildItem -Path $folder.FullName -Filter "*.jpg"

    # If there are more than 100 .jpg files, delete randomly until only 100 remain
    if ($jpgFiles.Count -gt 100) {
        $filesToDelete = $jpgFiles | Get-Random -Count ($jpgFiles.Count - 100)  # Select random files to delete
        $filesToDelete | ForEach-Object { Remove-Item $_.FullName }  # Delete the selected files
        Write-Output "Deleted $($filesToDelete.Count) files from $($folder.FullName), leaving 100 .jpg files."
    } else {
        Write-Output "The folder $($folder.FullName) already contains 100 or fewer .jpg files. No files deleted."
    }
}
