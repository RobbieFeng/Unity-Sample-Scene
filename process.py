import json
import os
import pathlib
import shutil
from datetime import datetime

from PIL import Image

PERCEPTION_PATH = next(open('PathToCapturedImages.txt')) # Generated after running the Unity project once
convertion_queue = []
remove_queue = []
success = True
count = 1
time = "";


class Result:
    def __init__(self):
        self.filename_bug = ""
        self.filename_bug_seg = ""
        self.filename_correct = ""
        self.filename_correct_seg = ""
        self.width = 0
        self.height = 0
        self.description = ""
        self.tag = ""
        self.victim = ""
        self.data_bug = []
        self.data_correct = []
        self.totalCount = 0


def walk(path):
    result = []
    w = os.walk(path)
    for (root, dirs, files) in w:
        for filename in files:
            if filename.endswith(".json") and "raw" in filename:
                result.append(os.path.join(root, filename))
    return result


# for perception
def walk2(path):
    result = {}
    w = os.walk(path)
    for (root, dirs, files) in w:
        for filename in files:
            if filename.endswith("frame_data.json"):
                path2 = os.path.join(root, filename)
                data = load_json(path2)
                frame = data["frame"]
                result[frame] = [data, root]
    return result


def load_json(filename):
    with open(filename, 'r') as f:
        data = json.load(f)
    return data


def combine(file, perception, is_bug, result):
    captures = perception[0]["captures"][0]
    annotations = captures["annotations"][0]
    if is_bug:
        try:
            result.filename_bug = time + str(count) + "_bug.jpg"
            convertion_queue.append([result.filename_bug, os.path.join(perception[1], captures["filename"]), True])
            result.filename_bug_seg = time + str(count) + "_bug_seg.png"
            convertion_queue.append(
                [result.filename_bug_seg, os.path.join(perception[1], annotations["filename"]), False])
            result.data_bug = annotations["instances"]
            return result
        except Exception as e:
            print("Error occurred while processing step " + str(file["step"]) + ".")
            raise e
    else:
        try:
            result.filename_correct = time + str(count) + "_cor.jpg"
            convertion_queue.append([result.filename_correct, os.path.join(perception[1], captures["filename"]), True])
            result.filename_correct_seg = time + str(count) + "_cor_seg.png"
            convertion_queue.append(
                [result.filename_correct_seg, os.path.join(perception[1], annotations["filename"]), False])
            result.width = file["pixelWidth"]
            result.height = file["pixelHeight"]
            result.description = file["desription"]
            result.tag = file["tag"]
            result.victim = file["victim"]
            result.data_correct = annotations["instances"]
            result.totalCount = len(result.data_correct)
            return result
        except Exception as e:
            print("Error occurred while processing step " + str(file["step"]) + ".")
            raise e


def move_one(command):
    global success
    origin_file = command[1]
    after_file = os.path.join(pathlib.Path(__file__).parent.resolve(), "out", command[0])
    if (command[2]):
        try:
            png_image = Image.open(origin_file).convert("RGB")
            png_image.save(after_file, "JPEG")
            remove_queue.append(origin_file)
        except Exception as e:
            print(f"Error converting {command[1]}: {str(e)}")
            success = False
    else:
        shutil.copy2(origin_file, after_file)
        remove_queue.append(origin_file)


def move_and_delete():
    for command in convertion_queue:
        if success:
            move_one(command)
            print(str(convertion_queue.index(command)) + "/" + str(len(convertion_queue)), end='\r')

    for command in remove_queue:
        if success:
            if "AppData" not in command:
                os.remove(command)


def save_json(data, path):
    with open(path, 'w') as f:
        json.dump(data, f, indent=4)


def delete_files_in_directory(directory_path):
    try:
        w = os.walk(directory_path)
        for (root, dirs, files) in w:
            for dir in dirs:
                if "solo" in dir:
                    shutil.rmtree(os.path.join(root, dir))

    except OSError:
        print("Error occurred while deleting screenshots.\nGo to " + PERCEPTION_PATH + " and delete them manually.")


def main():
    global count, time, PERCEPTION_PATH
    now = datetime.now()
    time = now.strftime("%Y%m%d%H%M_")


    perceptions = walk2(PERCEPTION_PATH)
    path = os.path.join(pathlib.Path(__file__).parent.resolve(), "out")
    files = walk(path)
    missingCout = 0
    i = 0
    for file in files:
        i += 1
        data = load_json(file)
        if data["frame"] + 1 not in perceptions or data["frame"] + 5 not in perceptions:
            missingCout += 1
            remove_queue.append(file)
            continue
        print(str(i) + "/" + str(len(files)), end='\r')
        perception1 = perceptions[data["frame"] + 1]
        perception2 = perceptions[data["frame"] + 5]
        try:
            result = Result()
            result = combine(data, perception1, True, result)
            result = combine(data, perception2, False, result)
        except:
            continue
        remove_queue.append(perception1[1])
        remove_queue.append(perception2[1])
        remove_queue.append(file)

        save_json(result.__dict__, os.path.join(path, time + str(count) + ".json"))
        count += 1
    print("Missing: " + str(missingCout) + "/" + str(len(files)))
    print("Moving files...", end='\r')
    move_and_delete()
    delete_files_in_directory(PERCEPTION_PATH)
    print("Done!                   ")

main()
