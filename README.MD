# XR-Twin4Industry
XRTwin4Industry is a research project that is part of the [JNI programme](https://www.irt-systemx.fr/en/research-programs/digital-twins-for-complex-industrial-systems/) of IRT SYSTEMX. the objective of this Framework is to construct a comprehensive methodology designed to tackle key challenges in the use of Extended Reality for industrial digital twin applications, specifically focusing on visualization, interaction, standardization, and interoperability. By conducting a state-of-the-art review, we identify the major obstacles and propose solutions through an integrated methodology that encompasses a tailored data model and system architecture aligned with the IEC 63278 standard and the Asset Administration Shell vision of Industry 4.0, ensuring compatibility with current industrial standards. To validate this approach, both a Proof of Concept and a real-world application scenario are developed, demonstrating the practical viability of the proposed framework. The design is further evaluated using User-Centered Design principles to ensure usability and effectiveness in industrial settings.
This repository contains the Proof of Concept source code and builds, as well as technical documentation.

## Related Research Paper
The associated research paper can be found [here](https://hal.science/hal-04915043/).

We later extended the methodology and prototype. to allow AR instructions, typically for education and maintenance purposes. The elements presented in this wiki are up to date with this new version.


## Installation
This project is made for **Microsoft Hololens 2 only**. However, users are free to adapt it to other hardwares.
To install the app, get the last build of the project in the build folder of this repository and copy it in your Hololens 2. Then, in the Hololens go to the the location where you copied the app folder, select the .appx file and press install.

Instructions to build from source can be found in the [wiki](https://github.com/IRT-SystemX/XRTwin4Industry/wiki).

## Features
Following the standardised methodology explained in the scientifical publication, this Proof of Concept allows to visualize and interact with Digital Twins in Augmented Reality, as well as to create and follow AR instructions procedures (for maintenance or learning).

XR Digital Twins can be loaded from a server where they have to be stored in the Asset Administration Shell format (A demo object with its AAS file and its 3d object can be found in this repository). Users can then visualize the Digitals Twins, their 3D models and their data. They can also interact with the Digital Twins: manipulate the objet (translation, rotation, scale), select components or operate/simulate the Physical Twin. 

AR instructions can be loaded from a QR code (leading to a compatible Asset Administration Shell file) placed in the real world. Instructions are then displayed in the 2W1H (What, Where How?) format, where AR User interface with texts and illustrations, as well as 3D objects placed in the world use the full potential of AR

## Wiki
This projects contains a complete [wiki](https://github.com/IRT-SystemX/XRTwin4Industry/wiki) where further information about the architecture, datamodel, usage can be found, as well as build instructions.

## To cite this work
Timothé Lacheroy, Abdelhadi Belfadel, Jean-Yves Didier. XRTwin4Industry: A Comprehensive Framework for Standardisation and Interoperability in XR-Enabled Industrial Digital Twins. Workshop Digital Twins Ecosystems and Applications (DIGITA), 23rd International Conference on Pervasive Computing and Communications (PerCom), Mar 2025, Washington, United States. ⟨hal-04915043⟩
