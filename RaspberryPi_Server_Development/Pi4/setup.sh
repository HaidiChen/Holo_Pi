#1. install the OS Raspbian Full

#2. update and upgrade the system
sudo apt update && sudo apt upgrade -y

#3. enable ssh
sudo apt install -y openssh-server

sudo sytemctl restart ssh

#4. install vim, git, libatlas-base-dev
sudo apt install -y vim git libatlas-base-dev

#5. copy the HoloPi project folder to the Desktop
cp -r HoloPi ~/Desktop/

#6. install openVINO toolkit
sudo mkdir -p /opt/intel/openvino
sudo tar -xf l_openvino_toolkit_runtime_raspbian_p_2019.3.334.tgz --strip 1 -C /opt/intel/openvino/

#7. install cmake
sudo apt install -y cmake

#8. setup openvino variables
echo "source /opt/intel/openvino/bin/setupvars.sh" >> ~/.bashrc

#9. add current user to users group 
#sudo usermod -a -G users "$(whoami)"

#10. install USB rules
bash /opt/intel/openvino/install_dependencies/install_NCS_udev_rules.sh

#11. build and run sample
cd ~/Desktop/
mkdir build && cd build
cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_CXX_FLAGS="-march=armv7-a" /opt/intel/openvino/deployment_tools/inference_engine/samples
make -j2 object_detection_sample_ssd

#12. download pre-trained face detection model
wget --no-check-certificate https://download.01.org/opencv/2019/open_model_zoo/R1/models_bin/face-detection-adas-0001/FP16/face-detection-adas-0001.bin
wget --no-check-certificate https://download.01.org/opencv/2019/open_model_zoo/R1/models_bin/face-detection-adas-0001/FP16/face-detection-adas-0001.xml
cp ~/Desktop/holo-pi/test-image.jpg .
./armv7l/Release/object_detection_sample_ssd -m face-detection-adas-0001.xml -d MYRIAD -i test-image.jpg

#13. check openCV installation
cp ~/Desktop/holo-pi/RaspberryPi_Server_Development/Pi4/openvino_fd_myriad.py .
python3 openvino_fd_myriad.py

#14. install python modules under virtual environment
cd ~/Desktop
sudo python3 -m pip install virtualenvwrapper
echo "export WORKON_HOME=$HOME/.virtualenvs/" >> ~/.bashrc
echo "export VIRTUALENVWRAPPER_PYTHON=/usr/bin/python3" >> ~/.bashrc
echo "source /usr/local/bin/virtualenvwrapper.sh" >> ~/.bashrc
source ~/.bashrc
mkvirtualenv ncsod
workon ncsod
pip install -r ~/Desktop/holo-pi/RaspberryPi_Server_Development/Pi4/requirements.txt
