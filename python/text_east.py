# Import required modules
import cv2 as cv
import argparse
import time
from utils.argument import str2bool
from utils.east import decode, drawResult
from utils.save import saveResult

############ Add argument parser for command line arguments ############
parser = argparse.ArgumentParser(
    description="Use this script to run TensorFlow implementation (https://github.com/argman/EAST) of EAST: An Efficient and Accurate Scene Text Detector (https://arxiv.org/abs/1704.03155v2)"
)
parser.add_argument(
    "--input",
    help="Path to input image or video file. Skip this argument to capture frames from a camera.",
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
    "--save",
    type=str2bool,
    nargs="?",
    const=True,
    default=False,
    help="Toggle of save the generated image.",
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

    # Create a new named window
    kWinName = "Text detection demo with EAST"
    outNames = []
    outNames.append("feature_fusion/Conv_7/Sigmoid")
    outNames.append("feature_fusion/concat_3")

    # Open a video file or an image file or a camera stream
    cap = cv.VideoCapture(args.input if args.input else 0)

    while cv.waitKey(1) < 0:
        # Save program start time
        start_time = time.time()

        # Read frame
        hasFrame, frame = cap.read()
        if not hasFrame:
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
        print("[INFO] EAST " + label)
        if args.info:
            cv.putText(
                frame, label, (0, 60), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255)
            )

        # Calculate processing time
        label = "Total process time: %.2f ms" % ((time.time() - start_time) * 1000)
        print("[INFO] " + label)
        if args.info:
            cv.putText(
                frame, label, (0, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255)
            )

        # Display the frame
        cv.imshow(kWinName, frame)

        # Save results
        if args.save and args.input:
            saveResult(frame, "text_east")


if __name__ == "__main__":
    main()
