# usage: ./opencv_download.sh [opencv_version (e.g. 3.4.5) default=3.4.5]
# Download the OpenCV source
cd ~
if [ "$1" == "" ]
then
    wget -O opencv.zip https://github.com/opencv/opencv/archive/3.4.5.zip
    wget -O opencv_contrib.zip https://github.com/opencv/opencv_contrib/archive/3.4.5.zip
else
    wget -O opencv.zip https://github.com/opencv/opencv/archive/$1.zip
    wget -O opencv.zip https://github.com/opencv/opencv_contrib/archive/$1.zip
fi
unzip opencv.zip
unzip opencv_contrib.zip
if [ "$1" == "" ]
then
    mv opencv-3.4.5 opencv
    mv opencv_contrib-3.4.5 opencv_contrib
else
    mv opencv-$1 opencv
    mv opencv_contrib-$1 opencv_contrib
fi
