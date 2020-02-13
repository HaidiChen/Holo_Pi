# usage:
# python3 HoloPi_Server.py -t [IP Address of the other Pi]
from flask import Flask, jsonify, request
from flask_restful import Resource, Api, reqparse
import werkzeug, os
from werkzeug.utils import secure_filename
import base64
import requests
from ncsrecognize import detectFromImage
import datetime
import argparse

ap = argparse.ArgumentParser()
ap.add_argument('-t', '--targetIp', required=True, help='the IP address of another pi')
args = vars(ap.parse_args())

if os.path.isdir('Images') == False:
    os.mkdir('Images')
    os.mkdir('Images/Uploaded')
    os.mkdir('Images/DetectionResults')

app = Flask(__name__)
api = Api(app)

UPLOAD_FOLDER = 'Images/Uploaded/'

parser = reqparse.RequestParser()
parser.add_argument('file',
        type=werkzeug.datastructures.FileStorage,
        location='files')
parser.add_argument('sharing', type=dict, action='append', location='json')
parser.add_argument('sourceIp', type=str, location='json')

sharingString = [] 
sourceIp = ''

class Connecting(Resource):
    def get(self):
        return 1 

class Detecting(Resource):
    def post(self):
        data = parser.parse_args()
        if data['file'] == None:
            return "no file"
        photo = data['file']

        if photo:
            filename = secure_filename(photo.filename) 
            d = datetime.datetime.now()
            directory = UPLOAD_FOLDER + d.date().isoformat()
            if os.path.isdir(directory) == False:
                os.mkdir(directory)

            filePath = os.path.join(directory, filename)

            photo.save(filePath)

            with open(filePath, mode="rb") as fi:
                imageString = base64.b64encode(fi.read()).decode()

            detectionResults = [{'ItemName': filename, 'ItemImage': imageString,
                'ItemDescription':'This is the original picture you uploaded.'}]

            detectionResults += detectFromImage(filePath)
            return jsonify(detectionResults)

class Sharing(Resource):
    def post(self):
        data = parser.parse_args()
        if data['sharing'] == None:
            return  

        #send the sharing string to other RP
        DestIP = args['targetIp']
        try:
            resp = requests.post("http://" + DestIP + ":5000/store", json={"sharing": data['sharing'], 'sourceIp':request.remote_addr})
        except :
            return 2
        else:
            if resp.json() == 1:
                return 1
            else:
                return 0

class Receiving(Resource):
    def get(self):
        global sharingString
        if len(sharingString) > 0:
            copy = [{'sourceIp':sourceIp}]
            copy += sharingString
            sharingString = []
            return jsonify(copy)
        else:
            return 0

class Storing(Resource):
    def post(self):
        global sharingString
        global sourceIp

        data = parser.parse_args()
        if data['sharing'] == None:
            return  

        # store the sharing data to the local variable
        sharingString += data['sharing']
        sourceIp = data['sourceIp']
        return 1

class Notifying(Resource):
    def get(self):
        if len(sharingString) > 0:
            return 1
        else:
            return 0
       
api.add_resource(Connecting, '/')
api.add_resource(Detecting, '/detect')
api.add_resource(Sharing, '/share')
api.add_resource(Receiving, '/receive')
api.add_resource(Storing, '/store')
api.add_resource(Notifying, '/notify')

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0')
