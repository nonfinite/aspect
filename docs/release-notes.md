# Version 1

* 1.8.0 - Added ability to select multiple images which can then be filtered by or copied to the clipboard.
* 1.7.0 - Added context menu to open the containing folder of the current image.
* 1.6.0 - Added context menu to copy the image, file, or file path.
* 1.5.1 - Added exception handling when loading animated gif to fall back to static image if loading fails.
* 1.5.0 - Images display correctly with exif rotation.
* 1.4.0 - Added playback controls to animated gifs.
* 1.3.1 - Fixed display of progress spinner while loading thumbnails to be completely visible.
* 1.3.0 - Added support for webp images.
* 1.2.0 - Added option to display thumbnails in the image list.
* 1.1.0 - Limited update checks to once per day.
* 1.0.0 - Initial release version.

# PreRelease Version 0

* 0.9.3 - Persistence warning is now hidden by default, so it doesn't flash on screen while the file list is loading.
* 0.9.2 - File list flyout no longer creates an overlay on the title bar which sometimes looked odd.
* 0.9.1 - Current settings-based sort is applied instead of always defaulting to Name.
* 0.9.0 - Changed display of animated gifs to use MediaElement for improved performance.
* 0.8.6 - Disabled rating control when SQLite database can't be loaded.
* 0.8.5 - Updated settings to no longer attempt to save while loading.
* 0.8.4 - Increased default width of the image list for nicer display.
* 0.8.3 - Updated file list view to always keep the current item in view.
* 0.8.2 - Changed icon to be more visible on dark taskbars.
* 0.8.1 - Updated to respect system minimum distance to start dragging the image.
* 0.8.0 - Improved display of image fit buttons to highlight mutually exclusive nature.
* 0.7.1 - Image fit mode is restored to the previously used mode when switching images after custom move/zoom.
* 0.7.0 - Added display of release notes and credits.
* 0.6.0 - Current sort order is saved in settings.
* 0.5.0 - Added an application icon.
* 0.4.0 - Associated the application with supported image formats.
* 0.3.1 - Fixed settings to persist between versions.
* 0.3.0 - Added logging warings and errors to a `log.txt` file by default,
  and allowed configuration of log file with environment variables:
  * ASPECT_LOG_FILE - sets the log file path and name.
  * ASPECT_LOG_LEVEL - sets the minimum log level to write to the file.
* 0.2.3 - Improved performance of file list rendering.
* 0.2.2 - Fixed file list vertical scrolling to actually function.
* 0.2.1 - Manual update process no longer failes if release notes aren't available.
* 0.2.0 - Added display of current version number.
* 0.1.0 - Basic functionality.
