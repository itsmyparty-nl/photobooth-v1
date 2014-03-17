/* size check output for language c */
/* DO NOT MODIFY THIS FILE - MODIFY ./testsizes-createsource.sh and testsizes-typelist.txt instead */

#include <stdio.h>

#include <gphoto2/gphoto2-port.h>

#include <gphoto2/gphoto2-camera.h>
#include <gphoto2/gphoto2-file.h>
#include <gphoto2/gphoto2-filesys.h>
#include <gphoto2/gphoto2-abilities-list.h>

int
main(void)
{
  printf("%s %d\n", "CameraAbilities", sizeof(CameraAbilities));
  printf("%s %d\n", "CameraText", sizeof(CameraText));
  printf("%s %d\n", "CameraFilePath", sizeof(CameraFilePath));
  return 0;
}
