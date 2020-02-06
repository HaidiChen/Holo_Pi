from mvnc import mvncapi as mvnc
import numpy as np
import cv2
import base64
import datetime
import os

# initialize the list of class labels our network was trained to 
# detect, then generate a set of bounding box colors for each class
CLASSES = (
        "background", "areoplane", "bicycle", "bird",
        "boat", "bottle", "bus", "car", "cat", "chair",
        "cow", "diningtable", "dog", "horse", "motorbike",
        "person", "pottedplant", "sheep", "sofa", "train", "tvmonitor"
        )
COLORS = np.random.uniform(0, 255, size=(len(CLASSES), 3))

# frame dimensions should be square
PREPROCESS_DIMS = (300, 300)
DISPLAY_DIMS = (900, 900)

# calculate the multiplier needed to scale the bounding boxes
DISP_MULTIPLIER = DISPLAY_DIMS[0] // PREPROCESS_DIMS[0]

def preprocess_image(input_image):
    # preprocess the image
    preprocessed = cv2.resize(input_image, PREPROCESS_DIMS)
    preprocessed = preprocessed - 127.5
    preprocessed = preprocessed * 0.007843
    preprocessed = preprocessed.astype(np.float16)

    # return the image to the calling function
    return preprocessed

def predict(image, graph):
    # preprocess the image
    image = preprocess_image(image)

    # send the image to the NCS and rrun a forward pass to grab the
    # network predictions
    graph.LoadTensor(image, None)
    (output, _) = graph.GetResult()

    # grab the number of valid object predictions from the output,
    # then initialize the list of predictions
    num_valid_boxes = int(output[0])
    predictions = []

    # loop over results
    for box_index in range(num_valid_boxes):
        # calculate the base index into our array so we can extract
        # bounding box information
        base_index = 7 + box_index * 7

        # boxes with non-finite (inf, nan, etc) numbers must be ignored
        if (not np.isfinite(output[base_index]) or
                not np.isfinite(output[base_index + 1]) or
                not np.isfinite(output[base_index + 2]) or
                not np.isfinite(output[base_index + 3]) or
                not np.isfinite(output[base_index + 4]) or
                not np.isfinite(output[base_index + 5]) or
                not np.isfinite(output[base_index + 6])
                ):
            continue

        # extract the image width and height and clip the boxes to the
        # image size in case network returns boxes outside of the image
        # boundaries
        (h, w) = image.shape[:2]
        x1 = max(0, int(output[base_index + 3] * w))
        y1 = max(0, int(output[base_index + 4] * h))
        x2 = min(w, int(output[base_index + 5] * w))
        y2 = min(h, int(output[base_index + 6] * h))

        # grab the prediction class label, confidence
        # and bounding box (x, y) coordinates
        pred_class = int(output[base_index + 1])
        pred_conf = output[base_index + 2]
        pred_boxpts = ((x1, y1), (x2, y2))

        # create prediction tuple and append the prediction 
        # to the prediction list
        prediction = (pred_class, pred_conf, pred_boxpts)
        predictions.append(prediction)

    # return the list of predictions to the calling function
    return predictions

def detectFromImage(ImageFilePath):
    # define necessary parameters
    graphPath = "mobilenetgraph"
    confidence = 0.6
    detectionResults = []

    # grab a list of all NCS devices plugged in to USB
    #print("[INFO] finding NCS devices...")
    devices = mvnc.EnumerateDevices()

    # if no devices found, exit the script
    if len(devices) == 0:
        #print("[INFO] No devices found, please plug in a NCS")
        quit()

    # use the first device since this is a simple test script
    # you will want to modify this if using multiple NCS devices
    #print("[INFO] found {} devices. device0 will be used.".format(len(devices)))
    device = mvnc.Device(devices[0])
    device.OpenDevice()

    # open the CNN graph file
    #print("loading the graph file into RPi memory...")
    with open(graphPath, mode="rb") as f:
        graph_in_memory = f.read()

    # load the graph into the NCS
    #print("[INFO] allocating the graph on the NCS...")
    graph = device.AllocateGraph(graph_in_memory)

    frame = cv2.imread(ImageFilePath)
    image_for_result = frame.copy()
    image_for_result = cv2.resize(image_for_result, DISPLAY_DIMS)

    predictions = predict(frame, graph)

    # loop over our predictions
    for (i, pred) in enumerate(predictions):
        # extract prediction data for readability
        (pred_class, pred_conf, pred_boxpts) = pred
        
        image_for_process = image_for_result.copy()
        detection_dict = {'ItemName':'', 'ItemImage':'', 'ItemDescription':''}

        # filter out weak detections by ensuring the 'confidence'
        # is greater than the minimum confidence
        if pred_conf > confidence:
            # print prediction to terminal
            #print("[INFO] Prediction #{}: class={}, confidence={},"
            #        " boxpoints={}".format(i, CLASSES[pred_class], pred_conf,
            #            pred_boxpts))

            # build a label consisting of the predicted class and 
            # associated probability
            label = "{}: {:.2f}%".format(CLASSES[pred_class],
                    pred_conf * 100)

            # extract information from the prediction boxpoints
            (ptA, ptB) = (pred_boxpts[0], pred_boxpts[1])
            ptA = (ptA[0] * DISP_MULTIPLIER, ptA[1] * DISP_MULTIPLIER)
            ptB = (ptB[0] * DISP_MULTIPLIER, ptB[1] * DISP_MULTIPLIER)
            (startX, startY) = (ptA[0], ptA[1])
            y = startY - 15 if startY - 15 > 15 else startY + 15

            #crop_x = ptA[0] - 30
            #crop_y = ptA[1] - 50
            #crop_height = ptB[1] - ptA[1] + 150
            #crop_width = ptB[0] - ptA[0] + 60

            # display the rectangle and label text
            cv2.rectangle(image_for_process, ptA, ptB,
                    COLORS[pred_class], 2)
            cv2.putText(image_for_process, label, (startX, y),
                    cv2.FONT_HERSHEY_SIMPLEX, 1, COLORS[pred_class], 3)

            d = datetime.datetime.now()
            directory = 'Images/DetectionResults/' + d.date().isoformat()
            if os.path.isdir(directory) == False:
                os.mkdir(directory)

            fileName = d.time().isoformat().replace(":", "-").replace(".", "-") + '.jpg'
            filePath = os.path.join(directory, fileName)
            cv2.imwrite(filePath, image_for_process)
            #cv2.imwrite(filePath, image_for_process[crop_y:crop_y+crop_height, crop_x:crop_x+crop_width])

            with open(filePath, "rb") as fi:
                imageString = base64.b64encode(fi.read())

            detection_dict['ItemName'] = label
            detection_dict['ItemImage'] = imageString
            detection_dict['ItemDescription'] = 'This is supposed to be a description of detected item.'
            detectionResults.append(detection_dict)


    # clean up the graph and device
    graph.DeallocateGraph()
    device.CloseDevice()

    return detectionResults
