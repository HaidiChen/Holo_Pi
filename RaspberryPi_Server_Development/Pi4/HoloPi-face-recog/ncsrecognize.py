# import the necessary packages
from imutils.video import VideoStream
from imutils.video import FPS
import numpy as np
import argparse
import imutils
import pickle
import time
import datetime
import cv2
import os
import base64

def detectFromImage(ImagePath):
    detectionResults = []
    confidence_threshold = 0.5

    # load our serialized face detector from disk
    #print("[INFO] loading face detector...")
    protoPath = 'face_detection_model/deploy.prototxt'
    modelPath = "face_detection_model/res10_300x300_ssd_iter_140000.caffemodel"
    detector = cv2.dnn.readNetFromCaffe(protoPath, modelPath)
    detector.setPreferableTarget(cv2.dnn.DNN_TARGET_MYRIAD)

    # load our serialized face embedding model from disk and set the
    # preferable backend to OpenCV
    #print("[INFO] loading face recognizer...")
    embedder = cv2.dnn.readNetFromTorch(
            'face_embedding_model/openface_nn4.small2.v1.t7')
    embedder.setPreferableBackend(cv2.dnn.DNN_BACKEND_OPENCV)

    # load the actual face recognition model along with the label encoder
    recognizer = pickle.loads(open('output/recognizer.pickle', "rb").read())
    le = pickle.loads(open('output/le.pickle', "rb").read())

    # loop over frames from the video file stream
    # grab the frame from the threaded video stream
    orig = cv2.imread(ImagePath)
    frame = orig.copy()

    # resize the frame to have a width of 600 pixels (while
    # maintaining the aspect ratio), and then grab the image
    # dimensions
    frame = imutils.resize(frame, width=600)
    (h, w) = frame.shape[:2]

    # construct a blob from the image
    imageBlob = cv2.dnn.blobFromImage(
        cv2.resize(frame, (300, 300)), 1.0, (300, 300),
        (104.0, 177.0, 123.0), swapRB=False, crop=False)

    # apply OpenCV's deep learning-based face detector to localize
    # faces in the input image
    detector.setInput(imageBlob)
    detections = detector.forward()

    # loop over the detections
    for i in range(0, detections.shape[2]):
        # extract the confidence (i.e., probability) associated with
        # the prediction
        confidence = detections[0, 0, i, 2]
        detection_dict = {}
        frame_copy = frame.copy()

        # filter out weak detections
        if confidence > confidence_threshold:
            # compute the (x, y)-coordinates of the bounding box for
            # the face
            box = detections[0, 0, i, 3:7] * np.array([w, h, w, h])
            (startX, startY, endX, endY) = box.astype("int")

            # extract the face ROI
            face = frame_copy[startY:endY, startX:endX]
            (fH, fW) = face.shape[:2]

            # ensure the face width and height are sufficiently large
            if fW < 20 or fH < 20:
                    continue

            # construct a blob for the face ROI, then pass the blob
            # through our face embedding model to obtain the 128-d
            # quantification of the face
            faceBlob = cv2.dnn.blobFromImage(cv2.resize(face,
                    (96, 96)), 1.0 / 255, (96, 96), (0, 0, 0),
                    swapRB=True, crop=False)
            embedder.setInput(faceBlob)
            vec = embedder.forward()

            # perform classification to recognize the face
            preds = recognizer.predict_proba(vec)[0]
            j = np.argmax(preds)
            proba = preds[j]
            name = le.classes_[j]

            # draw the bounding box of the face along with the
            # associated probability
            label = "{}: {:.2f}%".format(name, proba * 100)
            y = startY - 10 if startY - 10 > 10 else startY + 10
            cv2.rectangle(frame_copy, (startX, startY), (endX, endY),
                    (0, 0, 255), 2)
            cv2.putText(frame_copy, label, (startX, y),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 0, 255), 2)
            d = datetime.datetime.now()
            directory = 'Images/DetectionResults/' + d.date().isoformat()
            if os.path.isdir(directory) == False:
                os.mkdir(directory)

            fileName = d.time().isoformat().replace(":", "-").replace(".", "-") + '.jpg'
            filePath = os.path.join(directory, fileName)
            cv2.imwrite(filePath, frame_copy)
            with open(filePath, "rb") as fi:
                imageString = base64.b64encode(fi.read()).decode()

            detection_dict['ItemName'] = label
            detection_dict['ItemImage'] = imageString
            detection_dict['ItemDescription'] = 'This is supposed to be a description of detected item.'
            detectionResults.append(detection_dict)

    return detectionResults
