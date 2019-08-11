# Holo_Pi -- Cross-Device Project

Device1:
Microsoft HoloLens (1st generation)

Device2:
Raspberry Pi 3 model B

Device3:
Intel Movidius Neural Compute Stick

# Two parts: 
1. Hololens Client Development
2. RaspberryPi Server Development


# Basic workflow:

1. Hololens UWP App calls Web APIs provided by the RaspberryPi to send
a captured image to the RaspberryPi.

2. RaspberryPi which connected to one or several Intel Movidius Neural Compute Sticks
receives the image and store it in a specific local directory and
then using that image for image processing. Finally, return the processing
result as a response back to the Hololens. 

# Project demonstration videos:
goto the address: http://dcslab.cse.unt.edu/~haidi and you will see (if the server is not down) the link to the HoloPi project videos
