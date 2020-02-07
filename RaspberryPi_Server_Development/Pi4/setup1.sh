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
echo "run the setup2 script..."
exec bash
