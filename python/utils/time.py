import math


def transTime(tick, string):
    if tick >= 60000:
        mins = math.floor(tick / 60000)
        secs = math.floor((tick - mins * 60000) / 1000)
        msec = tick - mins * 60000 - secs * 1000
        print(
            string
            + str(mins)
            + " mins "
            + str(secs)
            + " secs "
            + str(math.floor(msec))
            + " ms"
        )
    elif tick >= 1000:
        secs = math.floor(tick / 1000)
        msec = tick - secs * 1000
        print(string + str(secs) + " secs " + str(math.floor(msec)) + " ms")
    else:
        print(string + str(math.floor(tick)) + " ms")
