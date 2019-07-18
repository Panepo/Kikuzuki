# Import required modules
import cv2 as cv
import argparse
import time
import pytesseract
from utils.argument import str2bool
from utils.save import saveResult

############ Add argument parser for command line arguments ############
parser = argparse.ArgumentParser(description="Use this script to run tesseract OCR.")
parser.add_argument(
    "--input",
    help="Path to input image or video file. Skip this argument to capture frames from a camera.",
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
    # pytesseract config
    config = "-l eng --oem 1 --psm 3"

    # Create a new named window
    kWinName = "OCR demo with tesseract"

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

        # Put OCR information
        text = pytesseract.image_to_string(frame, config=config)

        if len(text) > 0:
            cv.putText(
                frame, text, (0, 60), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255)
            )
            print("[INFO] text '{}' found".format(text))
        else:
            print("[INFO] No text found")

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
            saveResult(frame, "tess")


if __name__ == "__main__":
    main()
