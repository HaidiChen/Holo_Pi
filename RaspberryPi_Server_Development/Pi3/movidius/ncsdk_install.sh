sudo apt-get install git
cd ~
mkdir workspace
cd workspace
git clone https://github.com/movidius/ncsdk.git
git clone https://github.com/movidius/ncappzoo.git
cd ~/workspace/ncsdk
make install
