This is a part of network scraper used for personal purposes
This snippet scrapes Google based on set keywords and pages, turns result into HTMLDocument, and creats XMLDocument with Titles, links and webpage names.
Notes:
-this snippet does not include a MainWindow or any Main method
-no XAML configuration here (.NET Core is being used in my project)

To-do:
-lose out physical HTML file and use a stream instead (it is disposed along the loop and can't find the exact moment)
