# Import required modules
import cv2 as cv
import numpy as np
import math
import argparse
import pyrealsense2 as rs

############ Add argument parser for command line arguments ############
parser = argparse.ArgumentParser(
    description="Use this script to run TensorFlow implementation (https://github.com/argman/EAST) of EAST: An Efficient and Accurate Scene Text Detector (https://arxiv.org/abs/1704.03155v2)"
)
parser.add_argument(
    "--model", help="Path to a binary .pb file of model contains trained weights."
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
args = parser.parse_args()

############ Utility functions ############
def decode(scores, geometry, scoreThresh):
    detections = []
    confidences = []

    ############ CHECK DIMENSIONS AND SHAPES OF geometry AND scores ############
    assert len(scores.shape) == 4, "Incorrect dimensions of scores"
    assert len(geometry.shape) == 4, "Incorrect dimensions of geometry"
    assert scores.shape[0] == 1, "Invalid dimensions of scores"
    assert geometry.shape[0] == 1, "Invalid dimensions of geometry"
    assert scores.shape[1] == 1, "Invalid dimensions of scores"
    assert geometry.shape[1] == 5, "Invalid dimensions of geometry"
    assert (
        scores.shape[2] == geometry.shape[2]
    ), "Invalid dimensions of scores and geometry"
    assert (
        scores.shape[3] == geometry.shape[3]
    ), "Invalid dimensions of scores and geometry"
    height = scores.shape[2]
    width = scores.shape[3]
    for y in range(0, height):

        # Extract data from scores
        scoresData = scores[0][0][y]
        x0_data = geometry[0][0][y]
        x1_data = geometry[0][1][y]
        x2_data = geometry[0][2][y]
        x3_data = geometry[0][3][y]
        anglesData = geometry[0][4][y]
        for x in range(0, width):
            score = scoresData[x]

            # If score is lower than threshold score, move to next x
            if score < scoreThresh:
                continue

            # Calculate offset
            offsetX = x * 4.0
            offsetY = y * 4.0
            angle = anglesData[x]

            # Calculate cos and sin of angle
            cosA = math.cos(angle)
            sinA = math.sin(angle)
            h = x0_data[x] + x2_data[x]
            w = x1_data[x] + x3_data[x]

            # Calculate offset
            offset = [
                offsetX + cosA * x1_data[x] + sinA * x2_data[x],
                offsetY - sinA * x1_data[x] + cosA * x2_data[x],
            ]

            # Find points for rectangle
            p1 = (-sinA * h + offset[0], -cosA * h + offset[1])
            p3 = (-cosA * w + offset[0], sinA * w + offset[1])
            center = (0.5 * (p1[0] + p3[0]), 0.5 * (p1[1] + p3[1]))
            detections.append((center, (w, h), -1 * angle * 180.0 / math.pi))
            confidences.append(float(score))

    # Return detections and confidences
    return [detections, confidences]

def detectDevice():
    deviceDetect = False
    ctx = rs.context()
    devices = ctx.query_devices()

    for dev in devices:
        productName = str(dev.get_info(rs.camera_info.product_id))
        # print(productName)

        # DS 435 config
        if productName in "0B07":
            deviceDetect = True
            print("Connect device: RealSense D435")
            break

        # DS 415 config
        elif productName in "0AD3":
            deviceDetect = True
            print("Connect device: RealSense D415")
            break

    if deviceDetect is not True:
        raise Exception("No supported device was found")

def main():
    # Read and store arguments
    confThreshold = args.thr
    nmsThreshold = args.nms
    inpWidth = args.width
    inpHeight = args.height

    if args.model:
        model = args.model
    else:
        model = "frozen_east_text_detection.pb"

    detectDevice()
    pipeline = rs.pipeline()
    config = rs.config()
    config.enable_stream(rs.stream.color, 1280, 720, rs.format.bgr8, 30)
    pipeline.start(config)

    # Load network
    net = cv.dnn.readNet(model)

    # Create a new named window
    kWinName = "EAST: An Efficient and Accurate Scene Text Detector"
    cv.namedWindow(kWinName, cv.WINDOW_NORMAL)
    outNames = []
    outNames.append("feature_fusion/Conv_7/Sigmoid")
    outNames.append("feature_fusion/concat_3")

    flagCapture = False

    try:
        while True:
            # Read frame
            frames = pipeline.wait_for_frames()
            color_frame = frames.get_color_frame()
            if not color_frame:
                cv.waitKey()
                break

            frame = np.asanyarray(color_frame.get_data())
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

            # Apply NMS
            indices = cv.dnn.NMSBoxesRotated(
                boxes, confidences, confThreshold, nmsThreshold
            )
            for i in indices:
                # get 4 corners of the rotated rect
                vertices = cv.boxPoints(boxes[i[0]])
                # scale the bounding box coordinates based on the respective ratios
                for j in range(4):
                    vertices[j][0] *= rW
                    vertices[j][1] *= rH
                for j in range(4):
                    p1 = (vertices[j][0], vertices[j][1])
                    p2 = (vertices[(j + 1) % 4][0], vertices[(j + 1) % 4][1])
                    cv.line(frame, p1, p2, (0, 255, 0), 2)

            # Put efficiency information
            cv.putText(frame, label, (0, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255))

            # Display the frame
            cv.imshow(kWinName, frame)

            # Process screen capture
            if flagCapture:
                cv.imwrite("capture.png", frame, [int(cv.IMWRITE_PNG_COMPRESSION), 0])
                flagCapture = False

            getKey = cv.waitKey(1) & 0xFF
            if getKey is ord('c') or getKey is ord('C'):
                flagCapture = True
            elif getKey is ord('q') or getKey is ord('Q'):
                break

    except Exception as e:
        print(e)
        pass

    finally:
        # Stop streaming
        cv.destroyAllWindows()
        pipeline.stop()


if __name__ == "__main__":
    main()
