# Import required modules
import cv2 as cv
import math
import argparse
import time
import pytesseract

############ Add argument parser for command line arguments ############
parser = argparse.ArgumentParser(
    description="Use this script to run TensorFlow implementation (https://github.com/argman/EAST) of EAST: An Efficient and Accurate Scene Text Detector (https://arxiv.org/abs/1704.03155v2)"
)
parser.add_argument(
    "--input",
    help="Path to input image or video file. Skip this argument to capture frames from a camera.",
)
parser.add_argument("--save", type=bool, help="Toggle of save the generated image.")
args = parser.parse_args()

def main():
    # pytesseract config
    config = ("-l eng --oem 1 --psm 3")

    # Create a new named window
    kWinName = "OCR demo with tesseract"
    cv.namedWindow(kWinName, cv.WINDOW_NORMAL)

    # Open a video file or an image file or a camera stream
    cap = cv.VideoCapture(args.input if args.input else 0)

    while cv.waitKey(1) < 0:
        # Read frame
        hasFrame, frame = cap.read()
        if not hasFrame:
            cv.waitKey()
            break

        # Put OCR information
        text = pytesseract.image_to_string(frame, config=config)
        cv.putText(frame, text, (0, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255))
        print("decode text: " + text)

        # Display the frame
        cv.imshow(kWinName, frame)

        # Save results
        if args.save:
            fileName = "Output_" + time.strftime("%Y-%m-%d_%H%M%S-", time.localtime()) + '.png'
            cv.imwrite(fileName, frame, [int(cv.IMWRITE_PNG_COMPRESSION), 0])


if __name__ == "__main__":
    main()
