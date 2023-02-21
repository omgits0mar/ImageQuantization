# Image Quantization and Segmentation

This repository contains implementations for the following image processing techniques:
- Color quantization
- Image segmentation
- Image compression

## Color Quantization

The idea of color quantization is to reduce the number of colors in a full resolution digital color image (24 bits per pixel) to a smaller set of representative colors called color palette. This project solves color quantization as a problem of clustering points in three-dimensional space, where the points represent colors found in the original image and the three axes represent the three color channels.

The implemented algorithm uses the Single-linkage clustering method (also called nearest neighbor method), where the distance between two clusters is the minimum distance between an observation in one cluster and an observation in the other cluster, which is defined as the Euclidean Distance. This is done by constructing a fully-connected undirected weighted graph and applying the minimum spanning tree greedy algorithm.

## Image Segmentation

This project also includes an implementation of image segmentation using the K-means clustering algorithm.

## Image Compression

The implemented image compression technique is based on the color quantization algorithm. The idea is to reduce the number of colors in the image, and then encode the image using a run-length encoding method.

## Usage

To compress an image, run the program.cs script. Choose The image you need to compress and choose the 'K' number of clusters. You can enjoy the GUI while the code does his jobe to quantize and compress the size of your image.

## Documentation

For more details on the explanation ide, implementation, results and running time, please see the [ImageQuantization.docx](Image Quantization.docx)
