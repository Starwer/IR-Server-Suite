## Open a couple windows
notepad.exe
explorer.exe
## list the windows
Select-Window | ft –auto
## Activate Notepad
Select-Window notepad* | Set-WindowActive
## Close Explorer
Select-Window explorer | Select -First 1 | Remove-WIndow
## Run a few more copies of notepad
notepad; notepad; notepad; notepad;
## Move them around so we can see them all ... (Note the use of foreach with the incrementation)
$i = 1;$t = 100; Select-Window notepad | ForEach { Set-WindowPosition -X 20 -Y (($i++)*$t) -Window $_ }
## Put some text into them ...
Select-Window notepad | Send-Keys "this is a test"
## Close the first notepad window by pressing ALT+F4, and pressing Alt+N
## In this case, you don't have to worry about shifting focus to the popup because it's modal
## THE PROBLEM with sending keys like that is:
##    if there is no confirmation dialog because the file is unchanged, the Alt+N still gets sent
Select-Window notepad | Select -First 1 | Send-Keys "%{F4}%n"
## Close the next notepad window ... 
## By asking nicely (Remove-Window) and then hitting "n" for "Don't Save"
## If there are no popups, Select-ChldWindow returns nothing, and that's the end of it
Select-Window notepad | Select -First 1 | Remove-Window -Passthru | 
   Select-ChildWindow | Send-Keys "n"
## Close the next notepad window the hard way 
## Just to show off that our "Window" objects have a ProcessID and can be piped to kill
Select-Window notepad | Select -First 1 | kill
## A different way to confirm Don't Save (use CLICK instead of keyboard)
## Notice how I dive in through several layers of Select-Control to find the button?
## This can only work experimentally: 
## use SPY++, or run the line repeatedly, adding "|Select-Control" until you see the one you want
Select-Window notepad | Select -First 1 | Remove-Window -Passthru | 
   Select-childwindow | select-control| select-control| select-control -title "Do&n't Save" | Send-Click
## But now we have the new -Recurse parameter, so it's easy.  Just find the window you want and ...
Select-Window notepad | Select -First 1 | Remove-Window -Passthru | 
Select-childwindow | select-control -title "Do&n't Save"  -recurse | Send-Click