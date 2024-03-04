import os
from PIL import Image
import shutil

global total, done



def convert(current_directory):
    global done
    for filename in os.listdir(current_directory):
        if os.path.isdir(os.path.join(current_directory, filename)):
            convert(os.path.join(current_directory, filename))
        elif filename.endswith(".png"):
            png_file = os.path.join(current_directory, filename)
            jpg_file = os.path.splitext(png_file)[0] + ".jpg"

            try:
                # Open the PNG image and convert it to RGB mode
                png_image = Image.open(png_file).convert("RGB")

                # Save the image as JPG
                png_image.save(jpg_file, "JPEG")

                # Remove the original PNG file
                os.remove(png_file)

                #print(f"Converted {filename} to JPG.")
            except Exception as e:
                print(f"Error converting {filename}: {str(e)}")
        done += 1
        if done%10 == 0:
            print(str(done) + "/" + str(total), end='\r')

def find_and_copy_files(source_dir, dest_dir, quality):
    # Create the destination directory if it doesn't exist
    if not os.path.exists(dest_dir):
        os.makedirs(dest_dir)

    for root, dirs, files in os.walk(source_dir):
        for file in files:
            # Check if the file is a PNG or TIFF file
            if file.lower().endswith(('.png', '.tiff', '.tif', '.tga')):
                # Construct the source and destination file paths
                source_path = os.path.join(root, file)

                dest_path = os.path.join(dest_dir, os.path.relpath(source_path, source_dir))
                if source_path.startswith(dest_dir[:-8]):
                    continue

                # Create the subdirectories in the destination path if they don't exist
                os.makedirs(os.path.dirname(dest_path), exist_ok=True)

                # Copy the file to the destination directory
                reduce_image_quality(source_path, dest_path, quality)
                # shutil.copy2(source_path, dest_path)
                print(f"Copied {source_path} to {dest_path}")


def reduce_image_quality(image_path_old, image_path_new, quality_percentage):
    # Open the image
    image = Image.open(image_path_old)
    size = (32, 32)
    # Reduce the image quality
    new_image = image.resize(size)
    new_image.save(image_path_new)


def delete(folder):
    for filename in os.listdir(folder):
        file_path = os.path.join(folder, filename)
        try:
            if os.path.isfile(file_path) or os.path.islink(file_path):
                os.unlink(file_path)
            elif os.path.isdir(file_path):
                shutil.rmtree(file_path)
        except Exception as e:
            print('Failed to delete %s. Reason: %s' % (file_path, e))

choose = int(input("1: Resize   2: Clean   \n"))
folders = os.listdir(os.getcwd())
if "Assets" in folders:
    destination_directory = os.getcwd() + "\\Assets\\Resources\\Texture"
else:
    raise Exception("Put this script under Project folder")

if choose == 1:
    source_directory = os.getcwd() + "\\Assets"
    find_and_copy_files(source_directory, destination_directory, 10)

elif choose == 2:
    delete(destination_directory)
else:
    raise Exception("Invalid option")