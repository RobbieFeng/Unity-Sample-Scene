
import os
import shutil
from tkinter import *
from PIL import ImageTk, Image
import pathlib

def forward(img_no):
    # GLobal variable so that we can have
    # access and change the variable
    # whenever needed
    global label, label2,label3, label4, labelText, index
    global button_forward
    global button_back
    global button_exit
    global List_images
    global image1, image2, image3, image4
    img_no = min(img_no, len(List_path)-1)
    index = img_no
    # This is for clearing the screen so that
    # our next image can pop up
    image1 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_cor.jpg").resize(size))
    label = Label(image=image1)
    image2 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_bug.jpg").resize(size))
    label2 = Label(image=image2)
    image3 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_cor_seg.png").resize(sizeSmall))
    label3 = Label(image=image3)
    image4 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_bug_seg.png").resize(sizeSmall))
    label4 = Label(image=image4)
    labelText = Label(text=str(img_no+1)+"/"+str(len(List_path)))
    # as the list starts from 0 so we are
    # subtracting one
    label.grid(row=1, column=0, columnspan=3, rowspan=2)
    label2.grid(row=1, column=3, columnspan=3, rowspan=2)
    label3.grid(row=1, column=7, columnspan=1)
    label4.grid(row=2, column=7, columnspan=1)
    button_forward = Button(root, text="forward",
                        command=lambda: forward(img_no + 1))


    # img_no-1 as we want previous image when we click
    # back button
    button_back = Button(root, text="Back",
                         command=lambda: back(img_no - 1))
    button_delete = Button(root, text="Delete",
                           command=lambda: delete(img_no))


    # img_no+1 as we want the next image to pop up
    if img_no == len(List_path)-1:
        button_forward = Button(root, text="Forward",
                                state=DISABLED)
        root.bind('<Right>', lambda event: forward(img_no))
    root.bind('<Left>', lambda event: back(img_no - 1))
    root.bind('<Right>', lambda event: forward(img_no + 1))
    root.bind('<Delete>', lambda event: delete(img_no))
    root.bind('<Up>', lambda event: delete(img_no))
    # Placing the button in new grid
    button_back.grid(row=0, column=0)
    button_forward.grid(row=0, column=1)
    button_delete.grid(row=0, column=2)
    labelText.grid(row=0, column=3)



def back(img_no):
    # We will have global variable to access these
    # variable and change whenever needed
    global label , label2,label3, label4, labelText, index
    global button_forward
    global button_back
    global button_exit
    global image1 , image2, image3, image4

    img_no = max(img_no, 0)
    index = img_no
    # for clearing the image for new image to pop up
    image1 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_cor.jpg").resize(size))
    label = Label(image=image1)
    image2 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_bug.jpg").resize(size))
    label2 = Label(image=image2)
    image3 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_cor_seg.png").resize(sizeSmall))
    label3 = Label(image=image3)
    image4 = ImageTk.PhotoImage(Image.open(List_path[img_no] + "_bug_seg.png").resize(sizeSmall))
    label4 = Label(image=image4)

    labelText = Label(text=str(img_no+1)+"/"+str(len(List_path)))
    label.grid(row=1, column=0, columnspan=3, rowspan=2)
    label2.grid(row=1, column=3, columnspan=3, rowspan=2)
    label3.grid(row=1, column=7, columnspan=1)
    label4.grid(row=2, column=7, columnspan=1)

    button_forward = Button(root, text="forward",
                            command=lambda: forward(img_no + 1))
    button_back = Button(root, text="Back",
                         command=lambda: back(img_no - 1))
    button_delete = Button(root, text="Delete",
                           command=lambda: delete(img_no))
    root.bind('<Left>', lambda event: back(img_no - 1))
    root.bind('<Right>', lambda event: forward(img_no + 1))
    root.bind('<Delete>', lambda event: delete(img_no))
    root.bind('<Up>', lambda event: delete(img_no))

    # whenever the first image will be there we will
    # have the back button disabled
    if img_no == 0:
        button_back = Button(root, text="Back", state=DISABLED)
        root.bind('<Left>', lambda event: back(img_no))

    button_back.grid(row=0, column=0)
    button_forward.grid(row=0, column=1)
    button_delete.grid(row=0, column=2)
    labelText.grid(row=0, column=3)



def delete(img_no):
    global List_path
    os.remove(List_path[img_no]+"_cor.jpg")
    os.remove(List_path[img_no]+"_bug.jpg")
    os.remove(List_path[img_no]+"_cor_seg.png")
    os.remove(List_path[img_no]+"_bug_seg.png")
    os.remove(List_path[img_no]+".json")
    List_path.pop(img_no)
    forward(img_no)


def walk(path):
    print("Finding samples...")
    result = []
    w = os.walk(path)
    count = 0
    for (root, dirs, files) in w:
        for filename in files:
            if filename.endswith(".json") and "_" in  filename and "202" in filename and "filtered" not in root: # Change to 203 if ten years later, wow
                result.append(os.path.join(root, filename)[:-5])
                #count += 1 # Uncomment to limit number of samples
                if count > 10:
                    return result
    return result

def save():
    #move all previous samples to seperate folder
    print("Moving all previous samples to seperate folder...")
    path_save = pathlib.Path().resolve() /"out"/ "filtered"
    if not os.path.exists(path_save):
        os.mkdir(path_save)
    for i in range(index):
        path_save_temp = path_save / List_path[i].split("\\")[-1]
        shutil.move(List_path[i]+"_cor.jpg", str(path_save_temp) +"_cor.jpg")
        shutil.move(List_path[i]+"_bug.jpg", str(path_save_temp) +"_bug.jpg")
        shutil.move(List_path[i]+"_cor_seg.png", str(path_save_temp) +"_cor_seg.png")
        shutil.move(List_path[i]+"_bug_seg.png", str(path_save_temp) +"_bug_seg.png")
        shutil.move(List_path[i]+".json", str(path_save_temp) +".json")
    print("Done.")
    exit(0)




List_path =walk(pathlib.Path().resolve())

size = (700,700)
sizeSmall = (size[0]//2, size[1]//2)

# Calling the Tk (The initial constructor of tkinter)
root = Tk()

# We will make the title of our app as Image Viewer
root.title("Image Viewer")

# The geometry of the box which will be displayed
# on the scree
root.geometry(str(size[0]*2+100)+"x"+str(size[1]+100))

image1 = image2 = image3 = image4 = None
index = 0




print("Done. Window should pop up now.")
if len(List_path) == 0:
    raise Exception("No image found")

labelText = Label(text="0/"+str(len(List_path)))
labelText2 = Label(text="Up/Del to delete, Left/Right to navigate")
labelText2.grid(row=4, column=4)
button_save = Button(root, text="Move all previous samples to seperate folder", command=lambda: save())
button_save.grid(row=4, column=5)


forward(0)
root.mainloop()
