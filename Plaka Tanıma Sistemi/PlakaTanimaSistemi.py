import datetime

import cv2
from skimage import transform
import numpy as np
import tensorflow as tf
from keras.models import Sequential
from tensorflow.python.keras import layers, activations
from signalrcore.hub_connection_builder import HubConnectionBuilder
import VideoStream
import logging
import requests
import json
from datetime import date

class PlateData:
    def __init__(self,Plate,CheckinDate):
        self.Plate = Plate
        self.CheckinDate = CheckinDate
def listToJsonData(list):
    dictt = []
    for i in list:
        dictt.append(i.__dict__)
    try:
        json_data = json.dumps(dictt, sort_keys=True, default=str)
        return json_data
    except:
        print("JSON verisine çevrilemedi.")
        return ""
def controlPlate(plate):
    allLetter = "ABCDEFGHIJKLMNOPRSTUVYZ"
    if (len(plate) > 5):
        firstTwo = plate[0:2]
        if (firstTwo.isnumeric() == False):
            return False
        lastTwo = plate[-2:]
        if (lastTwo.isnumeric() == False):
            return False
        firstThree = plate[0:3]
        if (firstThree.isnumeric()):
            return False
        letterCount = 0
        for letter in allLetter:
            letterCount += plate.count(letter)
        if (letterCount > 3):
            return False
        elif (letterCount == 1 and letterCount == 3):
            if (len(plate) != 7):
                return False
        elif (letterCount == 2):
            if (len(plate) != 7 and len(plate) != 8):
                return False
        return True
    else:
        return False

def trytoConnectServer(url,hub_connection):
    try:
        hub_connection.start()
        response_API = requests.get('http://localhost:61818/api/Entry/AllEntries')
        if (response_API.status_code == 200):
            data = response_API.text
            parse_json = json.loads(data)
            for i in parse_json:
                if (i['checkoutDate'] is None):
                    inPlateList.append(i['plate'])
        else:
            print("Mevcut veriler cekilemedi. Status Code: " + response_API.status_code)
        return True
    except:
        print("Sunucu ile baglanti kurulamadi (GET)")
        print("Sunucu ile baglanti kurulamadi. (HUB)")
        return False
def sendData(hub_connection,data,url):
    if(data != None and hub_connection != None):
        hub_connection.send("SendMessageAsync", [data])
        print(f"{data} sunucuya iletildi.")
        if (lastplate not in inPlateList):
            inPlateList.append((lastplate))
        return 1
    else:
        try:
            json_data = listToJsonData(data)
            if(json_data != ""):
                response = requests.post(url, json= json_data)
                if(response.status_code == 200):
                    print("Kaydedilen plakalar sunucuya gonderildi.")
                    return 1
                else:
                    print("Kaydedilen plakalar sunucuya gonderilemedi.")
                    return -1
        except:
            print("Sunucu ile baglanti kurulamadi (POST).")
            return -1

def getPlate(cnts):
    lastPlate = tuple()
    lastPlateArea = None
    plateExist = False
    for c in cnts:
        peri = cv2.arcLength(c, True)
        approx = cv2.approxPolyDP(c, 0.02 * peri, True)
        if len(approx) == 4:
            if (plateExist == False):
                lastPlate = approx
                lastPlateArea = cv2.contourArea(c)
                plateExist = True
            else:
                possiblePlateArea = cv2.contourArea(c)
                if(possiblePlateArea == 0):
                    return lastPlate
                if (lastPlateArea / possiblePlateArea > 1 and lastPlateArea / possiblePlateArea < 2):
                    lastPlate = approx
    return lastPlate

def getPlateNumfromNum(num):
    if num < 10:
        return chr(num + 48)
    else:
        num += 55
        if num == 81:
            return 'R'
        elif num == 82:
            return 'S'
        elif num == 83:
            return 'T'
        elif num == 84:
            return 'U'
        elif num == 85:
            return 'V'
        elif num == 86:
            return 'Y'
        elif num == 87:
            return 'Z'
        else:
            return chr(num)

def createModel():
    model = Sequential()

    model.add(layers.Conv2D(filters=4, activation="elu", kernel_size=(5, 5), input_shape=(256, 256, 1)))
    model.add(layers.MaxPool2D(2, 2))
    model.add(layers.Conv2D(filters=8, activation="elu", kernel_size=(3, 3)))
    model.add(layers.MaxPool2D(2, 2))
    model.add(layers.Conv2D(filters=16, activation="elu", kernel_size=(5, 5)))
    model.add(layers.MaxPool2D(2, 2))
    model.add(layers.Conv2D(filters=32, activation="elu", kernel_size=(5, 5)))

    model.add(layers.Flatten())

    model.add(layers.Dense(50, activation="elu"))
    model.add(layers.Dense(100, activation="elu"))
    model.add(layers.Dense(100, activation="elu"))
    model.add(layers.Dense(50, activation="elu"))
    model.add(layers.Dense(33, activation="softmax"))

    optimizer = tf.keras.optimizers.Adamax(learning_rate=0.001)
    loss = tf.keras.losses.CategoricalCrossentropy()

    model.compile(optimizer=optimizer, loss=loss, metrics=["mse", "accuracy"])
    return model


def myResize(image, x, y):
    image = np.array(image).astype("uint8") / 255
    image = transform.resize(image, (x, y, 1))
    image = np.expand_dims(image, axis=0)
    return image


def returnPlateNumber(image, model):
    platenumber = ""
    contourList = list()
    if len(image.shape) == 3 and image.shape[2] == 3:
        image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    image = cv2.resize(image, (350, 83))
    blur = cv2.GaussianBlur(image, (3, 3), 0)
    thresh = cv2.threshold(blur, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)[1]
    kernel = np.ones((5, 5), np.uint8)
    opening = cv2.morphologyEx(thresh, cv2.MORPH_OPEN, kernel)
    cv2.imshow('Plaka', opening)
    cnts = cv2.findContours(opening, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    cnts = sorted(cnts, key=cv2.contourArea, reverse=True)
    for c in cnts:
        area = cv2.contourArea(c)
        x, y, w, h = cv2.boundingRect(c)
        if (area > 450 and area < 2100) and (h / w >= 1.2):
            contourList.append([x, y, w, h])
    modeList = {}
    if (len(contourList) == 0):
        return ""
    for contour in contourList:
        if (contour[1] in modeList):
            data = modeList.get(contour[1])
            modeList[contour[1]] = data + 1
        else:
            modeList[contour[1]] = 1
    mode = max(modeList, key=modeList.get)
    norContourList = [item for item in contourList if item[1] > (mode - 3) and item[1] < (mode + 3)]
    if (len(norContourList) == 7 or len(norContourList) == 8):
        #print("Baslangic")
        norContourList.sort(key=lambda obj: obj[0])
        #print("İlk Harf Sayisi:", len(contourList))
        #print("Son Harf Sayisi:", len(norContourList))
        #print("Mod:", mode)
        for i in norContourList:
            # print("X", i[0], "Y", i[1], "W:", i[2], "H:", i[3])
            ROI = 255 - opening[i[1]:i[1] + i[3], i[0]:i[0] + i[2]]
            ROI_ = myResize(ROI, 256, 256)
            pred = model.predict(ROI_)
            pred = np.argmax(pred)
            # filename = str(pred)+"_x:"+str(i[0])+"_y:"+str(i[1])+"_w:"+str(i[2])+"_h:"+str(i[3])
            # cv2.imwrite('C:\\Users\\karak\\Desktop\\HarfveSayilar\\{}.png'.format(filename), ROI)
            # print("Ekledim")
            platenum = getPlateNumfromNum(pred)
            platenumber += platenum
        #print(platenumber)
        #print("Bitis")
        contourList = []
        norContourList = []
        if (controlPlate(platenumber)):
            return platenumber
        else:
            return ""
    else:
        return ""


lastplate = ""
hub_connection = None
connectedStatus = False
url = 'http://localhost:61818/parksishub'
try:
     hub_connection = HubConnectionBuilder().with_url(url).build()
     hub_connection.on_open(lambda: print("Sunucu ile baglantı kuruldu."))
     hub_connection.on_close(lambda: print("Baglanti kapandi."))
     hub_connection.on_error(lambda data: print(f"Hata meydana geldi: {data.error}"))
     hub_connection.start()
     connectedStatus = True
except:
     print("Sunucu ile baglanti kurulamadi. (HUB)")
     connectedStatus = False
inPlateList = []
sendPlateList = []
post_API = 'http://localhost:61818/api/Entry/AddEntries'
try:
    response_API = requests.get('http://localhost:61818/api/Entry/AllEntries')
    if(response_API.status_code == 200):
        data = response_API.text
        parse_json = json.loads(data)
        for i in parse_json:
            if(i['checkoutDate'] is None):
                inPlateList.append(i['plate'])
    else:
        print("Mevcut veriler cekilemedi. Status Code: "+response_API.status_code)
except:
    print("Sunucu ile baglanti kurulamadi (GET)")
model = createModel()
model.build((1, 256, 256, 1))
model.load_weights("ysa_model.h5")
video_stream_widget = VideoStream.VideoStreamWidget()
while True:
    try:
        video_stream_widget.show_frame()
        frame = video_stream_widget.return_frame()
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        hist = cv2.equalizeHist(gray)
        blur = cv2.GaussianBlur(gray, (5, 5), 0)
        edged = cv2.Canny(blur, 10, 200)
        contours, _ = cv2.findContours(edged, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        contours = sorted(contours, key=cv2.contourArea, reverse=True)[:5]
        n_plate_cnt = getPlate(contours)
        if(len(n_plate_cnt) == 0):
            continue
        (x, y, w, h) = cv2.boundingRect(n_plate_cnt)
        possible_plate = gray[y:y + h, x:x + w]
        cv2.imshow('Plaka Ilk', possible_plate)
        cv2.drawContours(frame, [n_plate_cnt], -1, (0, 255, 0), 3)
        platenum = ""
        platenum = returnPlateNumber(possible_plate, model)
        if (platenum != ""):
             if (lastplate != platenum):
                 lastplate = platenum
                 print(lastplate)
                 if(hub_connection != None and connectedStatus == False):
                       connectedStatus = trytoConnectServer(url,hub_connection)
                 try:
                       sendData(hub_connection,lastplate,None)
                       if(lastplate in inPlateList):
                           inPlateList.remove(lastplate)
                       else:
                           inPlateList.append((lastplate))
                       if(len(sendPlateList) > 0):
                            response_msg = sendData(None,sendPlateList,post_API)
                            if(response_msg == 1):
                               sendPlateList.clear()

                 except:
                      print(f"{lastplate} sunucuya iletilemedi.")
                      connectedStatus = False
                      if(lastplate not in inPlateList):
                          inPlateList.append((lastplate))
                          sendPlateList.append(PlateData(lastplate,datetime.datetime.now()))
                          print("Gonderilmek uzere bellege eklendi.")
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    except:
        pass
hub_connection.stop()
cv2.destroyAllWindows()
