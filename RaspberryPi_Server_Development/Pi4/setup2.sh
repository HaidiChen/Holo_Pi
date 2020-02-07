#! /bin/bash

#9. install USB rules
bash /opt/intel/openvino/install_dependencies/install_NCS_udev_rules.sh

#10. build and run sample
cd ~/Desktop/
mkdir build && cd build
cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_CXX_FLAGS="-march=armv7-a" /opt/intel/openvino/deployment_tools/inference_engine/samples
make -j2 object_detection_sample_ssd

#11. download pre-trained face detection model
wget --no-check-certificate https://download.01.org/opencv/2019/open_model_zoo/R1/models_bin/face-detection-adas-0001/FP16/face-detection-adas-0001.bin
wget --no-check-certificate https://download.01.org/opencv/2019/open_model_zoo/R1/models_bin/face-detection-adas-0001/FP16/face-detection-adas-0001.xml
cp ~/Desktop/holo-pi/test-image.jpg .
./armv7l/Release/object_detection_sample_ssd -m face-detection-adas-0001.xml -d MYRIAD -i test-image.jpg

#12. check openCV installation
cp ~/Desktop/holo-pi/RaspberryPi_Server_Development/Pi4/openvino_fd_myriad.py .
python3 openvino_fd_myriad.py

#13. install python modules under virtual environment
sudo python3 -m pip install virtualenvwrapper
echo "export WORKON_HOME=$HOME/.virtualenvs/" >> ~/.bashrc
echo "export VIRTUALENVWRAPPER_PYTHON=/usr/bin/python3" >> ~/.bashrc
echo "source /usr/local/bin/virtualenvwrapper.sh" >> ~/.bashrc
echo "follow the setup3 file to finish the setup."
cd ~/Desktop/holo-pi/RaspberryPi_Server_Development/Pi4/
exec bash
