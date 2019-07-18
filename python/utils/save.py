import cv2 as cv
import time


def saveResult(image, name):
    fileName = (
        "./output/"
        + name
        + "_"
        + time.strftime("%Y-%m-%d_%H%M%S-", time.localtime())
        + ".png"
    )
    cv.imwrite(fileName, image, [int(cv.IMWRITE_PNG_COMPRESSION), 0])
    print("[INFO] saved results to {}".format(fileName))
