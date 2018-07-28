# Manga4you
This is UWP app on C# for read manga form any web-sites or zip archive.

# Installation 
You can:
- setup by MS Store link: https://www.microsoft.com/store/productId/9NTB302099T7 .
- download repository and build in VS.

# Add site for everyone
Open file https://github.com/ta-tikoma/Manga4you/blob/master/Manga/Assets/sites.cfg and send me you changes.  
File rules:  
` ## ` - site seporator  
`MangaKakalot.com` - site name  
`http://mangakakalot.com/home_json_search` - link for search on site  
`searchword=#word#&search_style=tentruyen` - post params for search  
`\"name\":\"(?<name>[^,]+)\",\"nameunsigned\":\"(?<link>[^\"]+)\",` - regex for search result  
`http://mangakakalot.com/manga/#link#` - manga link page  
`<span><a href=\"(?<link>[^\"]+)\" title=\"[^\"]+\">(?<name>[^<]+)</a></span>` - regex for chapters name and link  
`#link#` - chapter link  
`<img src=\"([^\"]+)\" alt=\"[^\"]+\" title=\"[^\"]+\" class='img_content' />` - regex for image  

# Features
- add link by site
- add notifications
- manga details
- add context menu on manga page:
  - save
  - width to screen
  - etc
