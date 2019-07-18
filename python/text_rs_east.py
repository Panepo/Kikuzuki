# Import required modules
import cv2 as cv
import numpy as np
import argparse
import time
from utils.argument import str2bool
from utils.east import decode, drawResult
from utils.save import saveResult
from utils.realsense import realsense, rsOptions

############ Add argument parser for command line arguments ############
parser = argparse.ArgumentParser(
    description="Use this script to run TensorFlow implementation (https://github.com/argman/EAST) of EAST: An Efficient and Accurate Scene Text Detector (https://arxiv.org/abs/1704.03155v2) with Intel RealSense camera."
)
parser.add_argument(
    "--model",
    default="../model/frozen_east_text_detection.pb",
    help="Path to a binary .pb file of model contains trained weights.",
)
parser.add_argument(
    "--width",
    type=int,
    default=320,
    help="Preprocess input image by resizing to a specific width. It should be multiple by 32.",
)
parser.add_argument(
    "--height",
    type=int,
    default=320,
    help="Preprocess input image by resizing to a specific height. It should be multiple by 32.",
)
parser.add_argument("--thr", type=float, default=0.5, help="Confidence threshold.")
parser.add_argument(
    "--nms", type=float, default=0.4, help="Non-maximum suppression threshold."
)
parser.add_argument(
    "--info",
    type=str2bool,
    nargs="?",
    const=True,
    default=False,
    help="Toggle of display information in images.",
)
args = parser.parse_args()


def main():
    # Read and store arguments
    confThreshold = args.thr
    nmsThreshold = args.nms
    inpWidth = args.width
    inpHeight = args.height

    # Load network
    net = cv.dnn.readNet(args.model)
    print("[INFO] East model loaded from {}".format(args.model))

    # Start RealSense Camera
    options = rsOptions()
    options.enableColor = True
    options.resColor = [1280, 720]
    rs = realsense(options)
    rs.deviceInitial()

    # Create a new named window
    kWinName = "Text detection demo with EAST"
    outNames = []
    outNames.append("feature_fusion/Conv_7/Sigmoid")
    outNames.append("feature_fusion/concat_3")

    flagCapture = False

    try:
        while True:
            # Save program start time
            start_time = time.time()

            # Read frame
            rs.getFrame()
            frame = rs.imageColor
            if not frame.any():
                cv.waitKey()
                break

            # Get frame height and width
            height_ = frame.shape[0]
            width_ = frame.shape[1]
            rW = width_ / float(inpWidth)
            rH = height_ / float(inpHeight)

            # Create a 4D blob from frame.
            blob = cv.dnn.blobFromImage(
                frame,
                1.0,
                (inpWidth, inpHeight),
                (123.68, 116.78, 103.94),
                swapRB=True,
                crop=False,
            )

            # Run the model
            net.setInput(blob)
            outs = net.forward(outNames)
            t, _ = net.getPerfProfile()
            label = "Inference time: %.2f ms" % (t * 1000.0 / cv.getTickFrequency())

            # Get scores and geometry
            scores = outs[0]
            geometry = outs[1]
            [boxes, confidences] = decode(scores, geometry, confThreshold)

            # Draw results
            drawResult(frame, rW, rH, boxes, confidences, confThreshold, nmsThreshold)

            # Put efficiency information
            if args.info:
                cv.putText(
                    frame, label, (0, 60), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255)
                )

            # Calculate processing time
            label = "Total process time: %.2f ms" % ((time.time() - start_time) * 1000)
            if args.info:
                cv.putText(
                    frame, label, (0, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255)
                )

            # Display the frame
            cv.imshow(kWinName, frame)

            # Process screen capture
            if flagCapture:
                print("[INFO] Screen captured")
                saveResult(frame, "text_rs_east")
                flagCapture = False

            # Keyboard commands
            getKey = cv.waitKey(1) & 0xFF
            if getKey is ord("c") or getKey is ord("C"):
                flagCapture = True
            elif getKey is ord("q") or getKey is ord("Q"):
                break

    except Exception as e:
        print(e)
        pass

    finally:
        # Stop streaming
        cv.destroyAllWindows()
        rs.pipeline.stop()


if __name__ == "__main__":
    main()
