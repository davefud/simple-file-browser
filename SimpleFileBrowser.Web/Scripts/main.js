$(function () {

    const BASE_URL = 'http://localhost:55749/api/',
          API_URL = BASE_URL + 'files/',
          UPLOAD_URL = BASE_URL + 'upload',
          DELETE_URL = API_URL + 'delete',
          MOVE_URL = API_URL + 'move',
          COPY_URL = API_URL + 'copy';

    // 	An array containing objects with information about the files.
    var filesAndFolders = [],
        hash = decodeURI(window.location.hash),
        path = '';

    // An event handler with calls the render function on every hashchange.
    // The render function will show the appropriate content of the page.
    $(window).on('hashchange', function () {
        render(decodeURI(window.location.hash));
    });

    if (hash) {
        path = hash.split('#browse/')[1] || '';

        // Set breadcrumbs when refreshing from url
        document.getElementById("breadcrumbs").innerHTML = createBreadcrumbs(path);
    } else {
        // Set default Home breadcrumb on empty window hash
        document.getElementById("breadcrumbs").innerHTML = createBreadcrumbs('', []);
    }

    renderBrowse(path);

    // Navigation
    function render(url) {

        // Get the hash keyword from the url.
        var path = url.split('/')[0] || '';

        clearUploadSuccessMsg();

        // Hide whatever page is currently shown.
        $('.main-content .page').removeClass('visible');

        var map = {

            // The "Homepage".
            '': function () {
                showItemsOrEmptyPage();
            },

            // Directory browsing
            '#browse': function() {

                var path = url.split('#browse/')[1].trim();

                if (!path) {
                    window.location.hash = '';
                }

                showItemsOrEmptyPage();
            }
        };

        // Execute the function depending on the hash keyword (stored in path).
        if (map[path]) {
            map[path]();
        }
        // If the hash keyword isn't listed in the above - render the error page.
        else {
            renderErrorPage();
        }

    }

    function renderBrowse(path) {
        $.getJSON(API_URL + encodeURIComponent(path), function (data) {

            // Write the data into our global variable.
            filesAndFolders = data

            // Call function to create HTML for all the items.
            generateAllItemsHTML(filesAndFolders);
    
            if (path !== '') {
                window.location.hash = 'browse/' + path;
            }

            $(window).trigger('hashchange');

            showItemsOrEmptyPage();

        });
    }

    function createBreadcrumbs(path) {
        var data = [],
            breadcrumbUrl = '',
            template = document.getElementById('template-breadcrumbs'),
            templateHtml = template.innerHTML,
            breadcrumbHtml = "";

        if (path) {
            data = path.split('/');
            breadcrumbUrl = "/#browse"
        }

        breadcrumbHtml = templateHtml.replace(/{{url}}/g, '/')
                                    .replace(/{{crumb}}/g, 'Home');
        
        for (var i = 0; i < data.length; i++) {
            breadcrumbUrl += '/' + data[i];
            breadcrumbHtml += templateHtml.replace(/{{url}}/g, breadcrumbUrl)
                                          .replace(/{{crumb}}/g, data[i]);
        }

        return breadcrumbHtml;
    }

    function createListHtml(data) {
        var template = document.getElementById('template-list-item');
        var templateHtml = template.innerHTML;
        var listHtml = "";

        for (var i = 0; i < data.length; i++) {
            listHtml += templateHtml.replace(/{{name}}/g, data[i]['name'])
                                    .replace(/{{lastModifiedTime}}/g, data[i]['lastModifiedTime'])
                                    .replace(/{{size}}/g, data[i]['size'])
                                    .replace(/{{extension}}/g, data[i]['extension'])
                                    .replace(/{{isDirectory}}/g, data[i]['isDirectory'])
                                    .replace(/{{downloadUrl}}/g, data[i]['downloadUrl'])
                                    .replace(/{{path}}/g, data[i]['path']);

            if (data[i]['isDirectory']) {
                listHtml = listHtml.replace(/{{image}}/g, "directory.jpg")
                                   .replace(/{{spanOrHref}}/g, "<span>" + data[i]['name'] + "</span>");

            } else {
                listHtml = listHtml.replace(/{{image}}/g, "file.png")
                                   .replace(/{{spanOrHref}}/g, "<a href='" + data[i]['downloadUrl'] + "' download=''>" + data[i]['name'] + "</a>");
            }

        }

        return listHtml;
    }

    // This function fills up the files & folders list & breadcrumbs nav via a javascript template.
    function generateAllItemsHTML(data) {

        var breadcrumbs = $('.breadcrumb-link'),
            list = $('.item-list');

        // Sets file and folder counter in header
        setItemCounter(data.length);

        // Generates the file and folders from the javascript template
        document.getElementById("item-list").innerHTML = createListHtml(data);
      
        // Sets the appropriate items or empty page visibility
        showItemsOrEmptyPage();

        /* 
        /   Set click handlers for the breadcrumbs, directory browsing, and buttons
        /   for all of the generated files and folders
        */
        breadcrumbs.find('a').on('click', function(e) {
            e.preventDefault();

            var hash = $(e.target).attr('href');

            var path =  hash === '/' ? '' :  hash.split('#browse/')[1];

            window.location.hash = 'browse/' + path;

            document.getElementById("breadcrumbs").innerHTML = createBreadcrumbs(path);
            
            renderBrowse(path);
        });

        list.find('.item-image').on('click', function(e) {
            e.preventDefault();

            var name = $(e.target).attr('title'),
                isDirectory = $(e.target).data('isdirectory'),
                path = $(e.target).data('path');

            if (!isDirectory) {
                return;
            }
            
            document.getElementById("breadcrumbs").innerHTML = createBreadcrumbs(path);

            renderBrowse(path);

        });

        list.find('.delete').on('click', function(e) {

            var path = $(this).data('path'),
                isDirectory = $(this).data('isdirectory');

            $.ajax({
                url: DELETE_URL + '?' + $.param({path: path, isDirectory: isDirectory}),
                type: 'DELETE',
                success: function(response) {
                    data.splice(data.findIndex(v => v.path === path), 1);
                    filesAndFolders = data;
                    generateAllItemsHTML(filesAndFolders);
                    clearUploadSuccessMsg();
                },
                error: function (response) {
                    alert(response.responseJSON);
                }
            });
        });

        list.find('.move').on('click', function(e) {

            var fromPath = $(this).data('path'),
                isDirectory = $(this).data('isdirectory');

            if (isDirectory)
            {
                alert('Moving Directories has not been implemented yet, but you can move files!');
                return;
            }

            var toPath = window.prompt(
                'Enter the complete path where you want to move the file.\n\n' + 
                '(Root directory starts with slash: ex: "/Root Directory")\n' + 
                '(No slash, relative to current directory: ex: "SubDirectory1/SubDirectory2")\n' + 
                '(Use relative paths: ex: "../../Root Directory")', '/');

            if (toPath) {
                moveRequest(fromPath, toPath, isDirectory);
            }
            
        });
    
        list.find('.copy').on('click', function(e) {
            var path = $(this).data('path'),
                isDirectory = $(this).data('isdirectory');
                
            if (isDirectory)
            {
                alert('Copying Directories not implemented yet, but you can copy files!');
                return;
            }

            copyRequest(path, isDirectory);
        });
    }

    function moveRequest(fromPath, toPath, isDirectory) {

        $.ajax({
            url: MOVE_URL + '?' + $.param({toPath: toPath, fromPath: fromPath, isDirectory: isDirectory}),
            type: 'POST',
            contentType: false,
            processData: false,
            success: function(response) {
                $('#updatePanelFile').addClass('alert-success')
                    .html('<span id="success-msg"><strong>Success, File Moved! </strong><a download="" href="' + response[0].downloadUrl + '">Download File</a></span>')
                    .show();
                    
                // Remove item deleted
                filesAndFolders.splice(filesAndFolders.findIndex(f => f.path === fromPath), 1);
                generateAllItemsHTML(filesAndFolders);
                window.scrollTo(0, 0);
            },
            error: function () {
                $('#updatePanelFile').addClass('alert-error').html('<strong>Error, Move File Failed!</strong> Please try again.').show();
                window.scrollTo(0, 0);
            }
        });
    }

    function copyRequest(path, isDirectory) {

        $.ajax({
            url: COPY_URL + '?' + $.param({path: path, isDirectory: isDirectory}),
            type: 'POST',
            contentType: false,
            processData: false,
            success: function(response) {
                $('#updatePanelFile').addClass('alert-success')
                    .html('<span id="success-msg"><strong>Success, File Copied!</strong> <a download="" href="' + response[0].downloadUrl + '">Download File</a></span>')
                    .show();

                // Add new file to list
                filesAndFolders.push.apply(filesAndFolders, response);
                generateAllItemsHTML(filesAndFolders);
                window.scrollTo(0, 0);
            },
            error: function () {
                $('#updatePanelFile').addClass('alert-error').html('<strong>Error, Copy File Failed!</strong> Please try again.').show();
                window.scrollTo(0, 0);
            }
        });
    }

    function showItemsOrEmptyPage() {

        var page = $('.all-items'),
            allItems = $('.all-items .item-list > li');

        // Show the page itself.
        // (the render function hides all pages so we need to show the one we want).
        page.addClass('visible');

        // Show empty page if we have no items to show.
        renderEmptyPage(allItems.length === 0);

    }

    function setItemCounter(count) {
        $('.items-count').text(count + ' items');
    }

    // Shows the error page.
    function renderErrorPage() {
        var page = $('.error');
        page.addClass('visible');
    }

    function renderEmptyPage(isVisible) {
        var page = $('.empty');
        if (isVisible) {
            page.addClass('visible');
        } else {
            page.removeClass('visible');
        }
    }

    function clearUploadSuccessMsg() {
        if ($('#updatePanelFile').hasClass('alert-success')) {
            $('#success-msg').remove();
            $('#updatePanelFile').hide();
        }
    }

    $('#file1').click(function() {
        clearUploadSuccessMsg();
    });

    $('#btnPostFile').click(function () {
        if ($('#file1').val() == '') {
            alert('Please select file');
            return;
        }

        var hash = decodeURI(window.location.hash);
        var path = hash.split('#browse/')[1];
        var formData = new FormData();
        var file = $('#file1')[0];

        formData.append('file', file.files[0]);

        $.ajax({
            url: UPLOAD_URL + '?' + $.param({path: path}),
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function(response) {
                $('#updatePanelFile').addClass('alert-success')
                    .html('<span id="success-msg"><strong>Upload Done! </strong><a download="" href="' + response[0].downloadUrl + '">Download File</a></span>')
                    .show();
                $('#file1').val(null);
                // Add new file to list
                filesAndFolders.push.apply(filesAndFolders, response);
                generateAllItemsHTML(filesAndFolders);
            },
            error: function () {
                $('#updatePanelFile').addClass('alert-error').html('<strong>Upload of File Failed!</strong> Please try again.').show();
            }
        });
    });

});