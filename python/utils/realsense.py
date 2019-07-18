# Import required modules
import pyrealsense2 as rs
import cv2 as cv
import numpy as np

"""
options:
options.enableColor       boolean   Toggle of color streaming
options.enableInfrared    boolean   Toggle of infrared streaming
options.enableDepth       boolean   Toggle of depth streaming
options.enableAlign       boolean   Toggle of depth of color/infrared alignment (color first)
options.enablePost        boolean   Toggle of enable depth image post processing
options.enableEqualize    boolean   Toggle of enable depth image equalization
options.depthMin          number    Number of minimum distance of depth equalization
options.depthMax          number    Number of maximum distance of depth equalization
options.enableIntrin      boolean   Toggle of get camera intrin matrix
options.resColor          array     resolution of color streaming
options.resInfrared       array     resolution of infrared streaming
options.resDepth          array     resolution of depth streaming
"""


class rsOptions:
    def __init__(self):
        self.enableColor = False
        self.enableInfrared = False
        self.enableDepth = False
        self.enableAlign = False
        self.enablePost = False
        self.enableEqualize = False
        self.depthMin = 0
        self.depthMax = 0
        self.enableIntrin = False
        self.resColor = [1920, 1080]
        self.resInfrared = [1280, 720]
        self.resDepth = [1280, 720]


class realsense:
    def __init__(self, options):
        self.options = options
        self.detected = False
        self.pipeline = None
        self.config = None
        self.align = None
        self.scale = None
        self.intrin = None
        self.frames = None
        self.imageColor = None
        self.imageInfrared = None
        self.imageDepth = None
        self.colorDepth = None

    def deviceDetect(self):
        ctx = rs.context()
        devices = ctx.query_devices()

        for dev in devices:
            productName = str(dev.get_info(rs.camera_info.product_id))
            # DS 435 config
            if productName in "0B07":
                self.detected = True
                print("[INFO] Connect device: RealSense D435")
                break
            # DS 415 config
            elif productName in "0AD3":
                self.detected = True
                print("[INFO] Connect device: RealSense D415")
                break

        if self.detected is not True:
            raise Exception("[ERROR] No supported device was found")

    def deviceInitial(self):
        self.deviceDetect()
        self.pipeline = rs.pipeline()
        self.config = rs.config()

        # Configure depth and color streams
        if self.options.enableColor is True:
            self.config.enable_stream(
                rs.stream.color,
                self.options.resColor[0],
                self.options.resColor[1],
                rs.format.bgr8,
                30,
            )

        if self.options.enableInfrared is True:
            self.config.enable_stream(
                rs.stream.infrared,
                self.options.resInfrared[0],
                self.options.resInfrared[1],
                rs.format.bgr8,
                30,
            )

        if self.options.enableDepth is True:
            self.config.enable_stream(
                rs.stream.depth,
                self.options.resDepth[0],
                self.options.resDepth[1],
                rs.format.z16,
                30,
            )

        # Start streaming
        cfg = self.pipeline.start(self.config)

        # Alignment
        if self.options.enableAlign is True:
            if self.options.enableColor is True:
                align_to = rs.stream.color
            elif self.options.enableInfrared is True:
                align_to = rs.stream.infrared

            self.align = rs.align(align_to)

        # Advanced settings
        dev = cfg.get_device()
        depth_sensor = dev.first_depth_sensor()
        if self.options.enableDepth is True and self.options.enablePost is True:
            depth_sensor.set_option(rs.option.visual_preset, 4)

        if self.options.enableIntrin is True:
            self.scale = depth_sensor.get_depth_scale()

            # Get intrinsics
            if self.options.enableColor is True:
                stream = cfg.get_stream(rs.stream.color)
                profile = stream.as_video_stream_profile()
            elif self.options.enableInfrared is True:
                stream = cfg.get_stream(rs.stream.infrared)
                profile = stream.as_video_stream_profile()

            self.intrin = profile.get_intrinsics()

    def procColorMap(self, input_image):
        inp = input_image.copy()

        if self.options.enableEqualize:
            scaleAlpha = 255 / (self.options.depthMax - self.options.depthMin)
            inp = cv.convertScaleAbs(
                inp, None, scaleAlpha, -1 * self.options.depthMin * scaleAlpha
            )
            mask = np.where((input_image == 0), 0, 1).astype("uint8")
            inp = inp * mask[:, :]
            """
        height, width = inp.shape
        for i in range(0, width):
            for j in range(0, height):
                if inp[j,i] is not 0:
                    inp[j,i] = int((inp[j,i] - self.options.depthMin) * scaleAlpha)
        """
        else:
            scaleAlpha = 255 / self.options.depthMax
            inp = cv.convertScaleAbs(inp, None, scaleAlpha, 0)

        depth_colormap = cv.applyColorMap(inp, cv.COLORMAP_JET)
        return depth_colormap

    def getFrame(self):
        self.frames = self.pipeline.wait_for_frames()

        if self.options.enableAlign is True:
            # Aligh frames
            aligned_frames = self.align.proccess(self.frames)

            if self.options.enableColor is True:
                color_frame = aligned_frames.get_color_frame()
                self.imageColor = np.asanyarray(color_frame.get_data())

            if self.options.enableInfrared is True:
                infrared_frame = aligned_frames.first(rs.stream.infrared)
                self.imageInfrared = np.asanyarray(infrared_frame.get_data())

            if self.options.enableDepth is True:
                depth_frame = aligned_frames.get_depth_frame()
                self.imageDepth = np.asanyarray(depth_frame.get_data())

                # colorize depth image
                self.colorDepth = self.procColorMap(self.imageDepth)
        else:
            if self.options.enableColor is True:
                color_frame = self.frames.get_color_frame()
                self.imageColor = np.asanyarray(color_frame.get_data())

            if self.options.enableInfrared is True:
                infrared_frame = self.frames.first(rs.stream.infrared)
                self.imageInfrared = np.asanyarray(infrared_frame.get_data())

            if self.options.enableDepth is True:
                depth_frame = self.frames.get_depth_frame()
                self.imageDepth = np.asanyarray(depth_frame.get_data())

                # colorize depth image
                self.colorDepth = self.procColorMap(self.imageDepth)
