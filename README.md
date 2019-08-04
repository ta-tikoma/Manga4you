# Manga4you
This is UWP app on C# for read manga form any web-sites or zip archive.

# Installation 
You can:
- setup by MS Store link: https://www.microsoft.com/store/productId/9NTB302099T7 .
- download repository and build in VS.

# Add site for everyone
Open file https://github.com/ta-tikoma/Manga4you/blob/master/Manga/Assets/sites.json and send me you changes.  
File rules: 
```json
  {
    "name": "site name",
    "search_link": "link for search on site",
    "search_post": "post params for search", 
    "search_regexp": "regex for search result",
    "chapters_link": "manga chapters link", 
    "chapters_regexp": "regex for chapters name and link",
    "pages_link": "link on manga chapter pages",
    "pages_regexp": "regex for image"
  }
```

# Features
- add link by site
- add notifications
- redesign main page
