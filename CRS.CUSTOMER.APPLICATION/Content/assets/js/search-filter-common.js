//#region LOCATION FILTER POPUP
function GetLocationFilterPopUp() {
    EnableLoaderFunction();
    document.body.classList.add('body-no-scroll');
    var locationfilterpopupContent = $('#locationfilterpopup-id').html();
    if (locationfilterpopupContent.trim() !== '') {
        var element = document.getElementById('drawer-search-by-area');
        if (element) {
            element.classList.remove('translate-y-full');
            DisableLoaderFunction();
            return false;
        }
    }
    $.ajax({
        type: 'GET',
        async: true,
        url: '/DashboardV2/GetLocationFilterPopUp',
        dataType: 'json',
        data: {
        },
        success: function (data) {
            if (!data || data.Code !== 0) {
                toastr.info(data?.Message);
                DisableLoaderFunction();
                return false;
            }
            $('#locationfilterpopup-id').html(data.PartialView);

            var divs = document.querySelectorAll('.select-location-store');
            // Add click event listeners to each div
            divs.forEach(function (div) {
                div.addEventListener('click', function () {
                    // Remove the 'bg-red' class from all divs
                    divs.forEach(function (d) {
                        d.classList.remove('active');
                    });
                    // Add the 'bg-red' class to the clicked div
                    this.classList.add('active');
                    var locationSelectValue = this.querySelector('.select-location-store .locationValue').getAttribute('data-info').trim();
                    var locationSelectLabel = this.querySelector('.select-location-store .locationValue').getAttribute('data-info-2').trim();
                    $('#filter-location-id').val(locationSelectValue);
                    $('#selected-locationlabel-id').html(locationSelectLabel);
                });
            });
            DisableLoaderFunction();
        },
        error: function (xhr, status, error) {
            toastr.info("Something went wrong. Please try again later.");
            DisableLoaderFunction();
            return false;
        }
    });
}

function CloseLocationFilterPopUp() {
    var element = document.getElementById('drawer-search-by-area');
    document.body.classList.remove('body-no-scroll');
    if (element) {
        element.classList.add('translate-y-full');
        return false;
    }
}
//#endregion

//#region PREFERENCE FILTER POPUP
function GetPreferenceFilterPopUp() {
    EnableLoaderFunction();
    document.body.classList.add('body-no-scroll');
    /* document.body.classList.remove('body-no-scroll');*/
    var CustomerCurrentLocationId = $('#current-location-id').val();
    var preferencefilterpopupContent = $('#preferencefilterpopUp-id').html();
    if (preferencefilterpopupContent.trim() !== '') {
        var element = document.getElementById('drawer-filter-location');
        if (element) {

            element.classList.remove('translate-y-full');
            DisableLoaderFunction();
            return false;
        }
    }
    var savedData = localStorage.getItem('PreferenceFilterHTMLContent');
    if (savedData) {
        savedData = JSON.parse(savedData);
        document.getElementById('preferencefilterpopUp-id').innerHTML = savedData.content; // Render the HTML content


        // Set input field values
        for (var inputId in savedData.inputValues) {
            if (inputId != "" && inputId != '') {
                if (document.getElementById(inputId)) {
                    document.getElementById(inputId).value = savedData.inputValues[inputId];
                }
            }
        }

        // Set checkbox states
        for (var checkboxId in savedData.checkboxStates) {
            if (document.getElementById(checkboxId)) {
                document.getElementById(checkboxId).checked = savedData.checkboxStates[checkboxId];
            }
        }

        for (var dropdownId in savedData.dropdownValues) {
            var selectDropdown = document.getElementById(dropdownId);
            if (selectDropdown) {
                for (var i = 0; i < selectDropdown.options.length; i++) {
                    if (selectDropdown.options[i].value === savedData.dropdownValues[dropdownId]) {
                        selectDropdown.options[i].selected = true;
                        break;
                    }
                }
            }
        }
        PreferenceFilterCommon();
        var ClubDetailMapData = localStorage.getItem('ClubDetailMapData');
        if (ClubDetailMapData) {
            ClubDetailMapData = JSON.parse(ClubDetailMapData);
            LoadGoogleMaps(ClubDetailMapData);
        }
        DisableLoaderFunction();
        return false;
    }
    else {
        $.ajax({
            type: 'GET',
            async: true,
            url: '/DashboardV2/GetPreferenceFilterPopUp',
            dataType: 'json',
            data: {
                LocationId: CustomerCurrentLocationId
            },
            success: function (data) {
                if (!data || data.Code !== 0) {
                    toastr.info(data?.Message);
                    DisableLoaderFunction();
                    return false;
                }
                $('#preferencefilterpopUp-id').html(data.PartialView);
                PreferenceFilterCommon();
                if (data.ClubDetailMapData != null) {
                    localStorage.setItem('ClubDetailMapData', JSON.stringify(data.ClubDetailMapData));
                    LoadGoogleMaps(data.ClubDetailMapData);
                }
                DisableLoaderFunction();
                return false;
            },
            error: function (xhr, status, error) {
                toastr.info("Something went wrong. Please try again later.");
                DisableLoaderFunction();
                return false;
            }
        });
    }
}

var map;
var marker;
var service;
var lastClickedCoordinates = null;
var infoWindows = [];
var trackUserInterval;
var customMarkerPosArray;



function PreferenceFilterCommon() {
    //#region 1
    var divs = document.querySelectorAll('.select-location-store');
    var inputField = document.querySelector('input[name="selectLocationStore"]'); // Select by name attribute
    var inputFieldsClub = document.querySelector('input[name="selectLocationClub"]'); // Select by name attribute
    // Add click event listeners to each div
    divs.forEach(function (div) {
        div.addEventListener('click', function () {
            // Remove the 'bg-red' class from all divs
            divs.forEach(function (d) {
                d.classList.remove('active');

            });
            // Add the 'bg-red' class to the clicked div
            this.classList.add('active');
            var locationSelectValue = this.querySelector('.select-location-store .locationValue')
                .textContent.trim();
            // Set the value of the input field to the content of the <p> tag
            inputField.value = locationSelectValue;

        });
    });
    //#endregion

    //#region 2
    var multiSelectValues = [];

    var multiSelect = document.querySelectorAll('.multi-select ');
    multiSelect.forEach(function (div) {
        // Get the value of the multi-select option
        var value = div.querySelector('.multi-select-value').innerText;

        // Check if the option has the 'active' class
        if (div.classList.contains('active')) {
            // If it's active, add its value to the array
            multiSelectValues.push(value);
        }
    });
    //console.log(multiSelectValues);

    // Add event listener to each multiSelect div
    multiSelect.forEach(function (div) {
        div.addEventListener('click', function () {
            // Toggle 'active' class for the clicked div
            this.classList.toggle('active');

            // Get the value of the clicked option
            var value = this.querySelector('.multi-select-value').getAttribute('data-value');

            // Check if the option is selected or deselected
            if (this.classList.contains('active')) {
                // If selected, add the value to the array
                multiSelectValues.push(value);
            } else {
                // If deselected, remove the value from the array
                var index = multiSelectValues.indexOf(value);
                if (index !== -1) {
                    multiSelectValues.splice(index, 1);
                }
            }
            $('#clubavailability-id').val(multiSelectValues);
            //console.log(multiSelectValues);
            // Log the array of selected values
        });
    });
    //#endregion

    //#region 3
    // Define searchbybusinesshours
    var searchbybusinesshours = document.querySelectorAll('.search-by-business-hours');

    // Define searchbybusinesshoursinputField
    //var searchbybusinesshoursinputField = document.querySelector('input[name="searchbybusinesshours"]');

    // Add event listener to each searchbybusinesshours div

    //modified

    /*    searchbybusinesshours.forEach(function (div) {
            div.addEventListener('click', function () {
                // Remove the 'active' class from all divs
                searchbybusinesshours.forEach(function (d) {
                    d.classList.remove('active');
                });
                // Add the 'active' class to the clicked div
                this.classList.add('active');
                if (this.classList.contains('contains-time-class')) {
                    var div = document.getElementById('time-div-id');
                    div.style.display = 'block';
                } else {
                    var div = document.getElementById('time-div-id');
                    div.style.display = 'none';
                }
                // Get the text content of the specific element within the clicked div
                var searchbybusinesshoursSelectValue = this.querySelector('.searchbybusinesshoursValue');
                $('#time-type-id').val(searchbybusinesshoursSelectValue.getAttribute('data-info'));
            });
        });*/

    searchbybusinesshours.forEach(function (div) {
        div.addEventListener('click', function () {
            // Check if the clicked div already has the 'active' class
            const isActive = this.classList.contains('active');

            // Remove the 'active' class from all divs
            searchbybusinesshours.forEach(function (d) {
                d.classList.remove('active');
            });

            // Toggle the 'active' class based on its previous state
            if (!isActive) {
                this.classList.add('active');
            }

            // Conditionally display the 'time-div-id' based on the presence of 'active' and 'contains-time-class'
            var div = document.getElementById('time-div-id');
            if (this.classList.contains('active') && this.classList.contains('contains-time-class')) {
                div.style.display = 'block';
            } else {
                div.style.display = 'none';
            }

            // Reset the value of '#time-type-id' if no div has 'active' class
            var activeDivExists = Array.from(searchbybusinesshours).some(d => d.classList.contains('active'));
            if (activeDivExists) {
                // Get the text content of the specific element within the active div
                var searchbybusinesshoursSelectValue = this.querySelector('.searchbybusinesshoursValue');
                $('#time-type-id').val(searchbybusinesshoursSelectValue.getAttribute('data-info'));
            } else {
                $('#time-type-id').val(''); // Reset or set to a default value
            }
        });
    });

    //#endregion

    //#region 6
    var showTimeLists = document.querySelectorAll('.showTimeList');

    showTimeLists.forEach(function (showTimeList) {
        showTimeList.addEventListener('click', function (event) {
            var timeList = showTimeList.nextElementSibling;
            if (timeList.style.display === "none" || timeList.style.display === "") {
                timeList.style.display = "block";
            } else {
                timeList.style.display = "none";
            }
            event.stopPropagation(); // Prevents the click event from bubbling up to the document
        });
    });

    document.addEventListener('click', function (event) {
        // Loop through each showTimeList
        showTimeLists.forEach(function (showTimeList) {
            var timeList = showTimeList.nextElementSibling;
            // Check if the click target is not the showTimeList, its timeList, or their descendants and close the timeList if it's open
            if (!showTimeList.contains(event.target) && !timeList.contains(event.target) && timeList.style
                .display === "block") {
                timeList.style.display = "none";
            }
        });
    });
    document.querySelectorAll('.timeList').forEach(item => {
        item.addEventListener('click', event => {
            if (event.currentTarget.classList.contains('disabled')) {
                return;
            }
            document.querySelectorAll('.timeList').forEach(item => {
                item.classList.remove('active');
            });
            event.currentTarget.classList.add('active');

            // Store the value if timeList has active class
            if (event.currentTarget.classList.contains('active')) {
                const timeValueElement = event.currentTarget.querySelector('.timeValue');
                if (timeValueElement) {
                    var timeValue = timeValueElement.getAttribute('data-info');
                    var selectedTimeDiv = document.getElementById("selected-time-id");
                    selectedTimeDiv.innerText = timeValueElement.innerHTML.trim();
                    $('#time-id').val(timeValue.trim());
                }
            }
        });
    });
    //#endregion

    //#region 8
    const tabs = document.querySelectorAll(".tab-preferance");
    const tabButtons = document.querySelectorAll(".preferance-tab");

    tabButtons.forEach((tabButton, index) => {
        tabButton.addEventListener("click", () => {
            // Hide all tabs and remove active class from buttons
            tabs.forEach((tab) => {
                tab.classList.add("hidden");
            });
            tabButtons.forEach((button) => {
                button.classList.remove("active");
            });
            tabButton.scrollIntoView({
                behavior: 'smooth', // Smooth scrolling
                inline: 'center' // Align the tab in the center of the view
            });

            // Show the selected tab and set active class on the clicked button
            tabs[index].classList.remove("hidden");
            tabButton.classList.add("active");
        });
    });
    //#endregion    
}

function initMap2() {
    map = new google.maps.Map(document.getElementById('map'), {
        zoom: 15, // Set the initial zoom level
        disableDefaultUI: true // Disable default UI elements
    });

    // Loop through the array of custom marker positions
    for (var i = 0; i < customMarkerPosArray.length; i++) {
        var customMarkerPos = customMarkerPosArray[i];
        createCustomMarker(customMarkerPos);
    }
    // Start tracking user's location
    trackUserLocation();
}

function trackUserLocation() {
    trackUserInterval = navigator.geolocation.watchPosition(function (position) {
        var pos = {
            lat: position.coords.latitude,
            lng: position.coords.longitude
        };

        if (!marker) {
            // Create marker for current location if not already created
            var markerIcon = {
                path: google.maps.SymbolPath.CIRCLE,
                fillColor: '#d75a8b',
                fillOpacity: 1,
                strokeColor: '#FFF',
                strokeWeight: 2,
                scale: 10
            };

            marker = new google.maps.Marker({
                position: pos,
                map: map,
                icon: markerIcon
            });
        } else {
            // Update marker position
            marker.setPosition(pos);
        }

        // Optionally, center the map on the user's position
        map.setCenter(pos);
    }, handleLocationError);
}

function stopTrackingUserLocation() {
    if (trackUserInterval) {
        navigator.geolocation.clearWatch(trackUserInterval);
        trackUserInterval = null;
    }
}

function searchNearby(location) {
    service.nearbySearch({
        location: location,
        radius: 500, // Search radius in meters
        type: ['restaurant'] // Only search for restaurants
    }, processResults);
}

function processResults(results, status) {
    if (status === google.maps.places.PlacesServiceStatus.OK) {
        for (var i = 0; i < results.length; i++) {
            createMarker(results[i]);
        }
    }
}

function createMarker(place) {
    var marker = new google.maps.Marker({
        map: map,
        position: place.geometry.location,
        title: place.name,
        icon: {
            scaledSize: new google.maps.Size(100, 100) // Scaled size of the marker icon
        }
    });
    // Add click listener to show place details
    // Add click listener to show place details
    google.maps.event.addListener(map, 'click', function (event) {
        var data = service.getDetails({
            placeId: event.placeId // Change event.place_id to event.placeId
        })
    });

}

function createCustomMarker(customMarkerPos) {
    var customMarker = new google.maps.Marker({
        position: customMarkerPos,
        map: map,
        title: 'Custom Marker',
        icon: {
            scaledSize: new google.maps.Size(100, 100) // Scaled size of the marker icon
        }
    });


    // Create info window for the custom marker

    // Add click event listener to show info window
    customMarker.addListener('click', function () {
        if (customMarkerPos.clubNameEnglish) {
            var clubCard = document.getElementById('map-card');
            clubCard.style.display = 'flex'; // Show the map card
            var clubImage = document.getElementById('clubImage');
            var clubNameEng = document.getElementById('clubNameEng');
            var clubNameJpn = document.getElementById('clubNamejpn');
            var rating = document.getElementById('rating');
            var clubUrl = document.getElementById('clubUrl');
            var ratingElement = document.getElementById('rating-label-id');

        } else {
            var clubCard = document.getElementById('map-card');
            clubCard.style.display = 'none'; // Hide the map card if clubNameEng is empty
        }


        clubImage.src = customMarkerPos.clubLogo;
        clubNameEng.textContent = customMarkerPos.clubNameEnglish;
        clubNameJpn.textContent = customMarkerPos.clubNameJapanese;
        rating.textContent = customMarkerPos.ratingScale;
        clubUrl.href = customMarkerPos.URL;
        ratingElement.innerHTML = generateStarIcons(customMarkerPos.ratingScale);
    });

}

function generateStarIcons(rating) {
    const fullStars = Math.floor(rating);
    const halfStar = rating % 1 !== 0;
    const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);

    let starsHTML = '';
    for (let i = 0; i < fullStars; i++) {
        starsHTML += '<i class="las la-star"></i>';
    }
    if (halfStar) {
        starsHTML += '<i class="las la-star-half-alt"></i>';
    }
    for (let i = 0; i < emptyStars; i++) {
        starsHTML += '<i class="lar la-star"></i>';
    }

    return starsHTML;
}

function handleLocationError(error) {
    console.error("Error getting user's location:", error);
}

function closeAllInfoWindows() {
    for (var i = 0; i < infoWindows.length; i++) {
        infoWindows[i].close();
    }
}

function closeAllInfoWindowsExcept(exceptInfoWindow) {
    for (var i = 0; i < infoWindows.length; i++) {
        if (infoWindows[i] !== exceptInfoWindow) {
            infoWindows[i].close();
        }
    }
}

// Function to set user's current location
function setUserLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var pos = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };

            map.setCenter(pos);
            map.setZoom(15); // Set the zoom level to 24

            // Create marker for current location
            var markerIcon = {
                path: google.maps.SymbolPath.CIRCLE,
                fillColor: '#ff0000',
                fillOpacity: 0.8,
                strokeColor: '#FFF',
                strokeWeight: 2,
                scale: 10
            };

            if (marker) {
                marker.setPosition(pos);
            } else {
                marker = new google.maps.Marker({
                    position: pos,
                    map: map,
                    icon: markerIcon
                });
            }
        }, handleLocationError);
    } else {
        console.error("Geolocation is not supported by this browser");
    }
}

function LoadGoogleMaps(ClubDetailMapData) {
    customMarkerPosArray = convertToCustomMarkerArray(ClubDetailMapData);
    var script = document.createElement('script');
    //script.src = "https://maps.googleapis.com/maps/api/js?key=AIzaSyCVqzKbK_YObo2ivCYETgRkFCdjCFs2aQA&callback=initMap2";
    script.src = "/Home/GetGoogleMapData";
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
    initMap2();
}

function convertToCustomMarkerArray(data) {
    var customMarkerPosArray2 = [];
    var markerObj = {};
    const finalData = data.map((val) => {
        const newObj = {};
        val.forEach((item) => {
            newObj[item.Key] = item.Value;
        });
        return newObj;
    });
    customMarkerPosArray2.push(...finalData);
    return customMarkerPosArray2;
}

function preventDefault(event) {
    event.preventDefault();
}
function ClosePreferenceFilterPopUp() {
    document.body.classList.remove('body-no-scroll');
    var element = document.getElementById('drawer-filter-location');
    if (element) {
        element.classList.add('translate-y-full');
        //document.body.addEventListener('touchmove', preventDefault, { passive: false });
        return false;
    }
}
//#endregion

//#region 1
function LocationFunction(i) {
    //window.location.href = "/LocationManagement/Index?LocationId=" + i;
    window.location.href = "/area" + i;
}
//#endregion

//#region 2
function ClubFilterSubmitButton() {
    EnableLoaderFunction();
    getSelectedCheckboxValues('search-by-store-category', 'club-category-id', 'club-category-class');// Call the function for the first set of checkboxes
    getSelectedCheckboxValues('search-by-plan', 'price-id', 'plan-price-class');// Call the function for the second set of checkboxes
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    ManagePreferenceFilterHTMLContent();
    var form = document.getElementById("club-filter-form-id");
    form.action = "/search" + locationId + "/?scftab=01";
    form.submit();
    DisableLoaderFunction();
}

function HostFilterSubmitButton() {
    EnableLoaderFunction();
    getSelectedCheckboxValues('choose-your-height', 'host-height-id', 'host-height');// Call the function for the first set of checkboxes
    getSelectedCheckboxValues('select-age', 'host-age-id', 'host-age');// Call the function for the second set of checkboxes
    getSelectedCheckboxValues('choose-blood-type', 'host-bloodtype-id', 'host-blood-type');// Call the function for the second set of checkboxes
    getSelectedCheckboxValues('hostConstellationGroup', 'host-constellationgroup-id', 'host-constellation-group-class');// Call the function for the second set of checkboxes
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    $('#host-occupation-id').val($('#host-occupation-ddl-id').val());
    ManagePreferenceFilterHTMLContent();
    var form = document.getElementById("host-filter-form-id");
    form.action = "/search" + locationId + "/?scftab=02";
    form.submit();
    DisableLoaderFunction();
}

function getSelectedCheckboxValues(checkboxName, targetElementId, checkboxClass) {
    var checkboxes = document.querySelectorAll('input[type="checkbox"][name="' + checkboxName + '"]');
    var selectedValues = [];
    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked && checkbox.classList.contains(checkboxClass)) {
            selectedValues.push(checkbox.value);
        }
    });
    document.getElementById(targetElementId).value = selectedValues.join(', ');
}

function ManagePreferenceFilterHTMLContent() {
    var content = document.getElementById('preferencefilterpopUp-id').innerHTML;
    var inputValues = {};
    var checkboxStates = {};
    var dropdownValues = {};
    document.querySelectorAll('input, select').forEach(function (input) {
        if (input.tagName === 'INPUT') {
            if (input.type === 'text' || input.type === 'search') {
                inputValues[input.id] = input.value; // Store input field values
            } else if (input.type === 'checkbox') {
                checkboxStates[input.id] = input.checked; // Store checkbox states
            }
        }
        else if (input.tagName === 'SELECT') {
            dropdownValues[input.id] = input.value; // Store dropdown values
        }
    });
    var savedData = {
        content: content,
        inputValues: inputValues,
        checkboxStates: checkboxStates,
        dropdownValues: dropdownValues
    };
    //localStorage.setItem('PreferenceFilterHTMLContent', content);
    localStorage.setItem('PreferenceFilterHTMLContent', JSON.stringify(savedData)); // Store the data
}
//#endregion
function preventDefault(event) {
    event.preventDefault();
}
function EnableLoaderFunction() {
    document.getElementById('loader-id-v2').style.display = 'flex';
    document.body.addEventListener('touchmove', preventDefault, { passive: false });
    document.body.classList.add('no-scroll-loader');
}
function DisableLoaderFunction() {
    document.getElementById('loader-id-v2').style.display = 'none';
    document.body.removeEventListener('touchmove', preventDefault, { passive: false });
    document.body.classList.remove('no-scroll-loader');
}



//#region date/time filter
async function InitiateDateTimeFilterPopupFunction() {
    EnableLoaderFunction();
    document.body.classList.add('body-no-scroll');
    document.getElementById("DateTimeFilter-Id").classList.remove("disable-click");
    var locationfilterpopupContent = $('#datetimefilterpopup-id').html();
    if (locationfilterpopupContent.trim() !== '') {
        var element = document.getElementById('drawer-date-time');
        if (element) {
            element.classList.remove('translate-y-full');
            DisableLoaderFunction();
            return false;
        }
    }

    var savedData = localStorage.getItem('DateTimeFilterHTMLContent');
    if (savedData) {
        savedData = JSON.parse(savedData);
        document.getElementById('datetimefilterpopup-id').innerHTML = savedData.content; // Render the HTML content


        // Set input field values
        for (var inputId in savedData.inputValues) {
            if (inputId != "" && inputId != '') {
                if (document.getElementById(inputId)) {
                    document.getElementById(inputId).value = savedData.inputValues[inputId];
                }
            }
        }

        // Set checkbox states
        for (var checkboxId in savedData.checkboxStates) {
            if (document.getElementById(checkboxId)) {
                document.getElementById(checkboxId).checked = savedData.checkboxStates[checkboxId];
            }
        }
        DisableLoaderFunction();
        DateTimeFilterCommon();
        return false;
    }

    $.ajax({
        type: 'GET',
        async: true,
        url: '/DashboardV2/InitiateDateTimeFilterPopup',
        dataType: 'json',
        data: {
        },
        success: function (data) {
            if (!data || data.Code !== 0) {
                toastr.info(data?.Message);
                DisableLoaderFunction();
                return false;
            }
            $('#datetimefilterpopup-id').html(data.PartialView);
            DateTimeFilterCommon();
            DisableLoaderFunction();
        },
        error: function (xhr, status, error) {
            toastr.info("Something went wrong. Please try again later.");
            DisableLoaderFunction();
            return false;
        }
    });
}

function DateTimeFilterCommon() {
    jQuery(function ($) {
        $.datepicker.regional['ja'] = {
            closeText: '閉じる',
            prevText: '&#x3c;前',
            nextText: '次&#x3e;',
            currentText: '今日',
            monthNames: ['1月', '2月', '3月', '4月', '5月', '6月',
                '7月', '8月', '9月', '10月', '11月', '12月'
            ],
            monthNamesShort: ['1月', '2月', '3月', '4月', '5月', '6月',
                '7月', '8月', '9月', '10月', '11月', '12月'
            ],
            dayNames: ['日曜日', '月曜日', '火曜日', '水曜日', '木曜日', '金曜日', '土曜日'],
            dayNamesShort: ['日', '月', '火', '水', '木', '金', '土',],
            dayNamesMin: ['日', '月', '火', '水', '木', '金', '土',],
            weekHeader: '週',
            dateFormat: 'yy/mm/dd',
            firstDay: 0,
            isRTL: false,
            showMonthAfterYear: true,
            yearSuffix: '年'
        };
        $.datepicker.setDefaults($.datepicker.regional['ja']);
        var currentDate = new Date();
        var maxDate = new Date(currentDate.getTime() + (14 * 24 * 60 * 60 * 1000)); // 15 days from today

        var selectedHtmlDate = $('#main-date-id').html();
        var selectedDate = null;
        if (selectedHtmlDate != null && selectedHtmlDate != '') {
            $('#search-date-id').val(selectedHtmlDate.trim());
            selectedDate = new Date(selectedHtmlDate);
        }
        else {
            selectedDate = currentDate;
        }

        $("#datepicker2").datepicker({
            minDate: currentDate,
            maxDate: maxDate,
            defaultDate: selectedDate,
            onSelect: function (dateText, inst) {
                inst.inline = true; // Set datepicker to inline mode
                $('#search-date-id').val(dateText.trim());
                $('#main-date-id').html(dateText.trim());
            }
        });
        $("#datepicker2").datepicker("show");
        //$("#datepicker2").datepicker({
        //    defaultDate: null
        //});
        // Open the calendar by default
        // Update selected date in datevalue element

    });
    initTimeFunction2();
    initPeopleFunction2();
}

function ManageDateTimeFilterHTMLContent() {
    var content = document.getElementById('datetimefilterpopup-id').innerHTML;

    //test
    // Create a temporary element to hold the content
    var tempDiv = document.createElement('div');
    tempDiv.innerHTML = content;

    // Create a new div element to replace datepicker2
    var newDatepicker2Element = document.createElement('div');
    newDatepicker2Element.id = 'datepicker2';
    newDatepicker2Element.style.marginTop = '10px';

    // Replace the existing datepicker2 with the new div element
    var oldDatepicker2Element = tempDiv.querySelector('#datepicker2');
    if (oldDatepicker2Element) {
        oldDatepicker2Element.parentNode.replaceChild(newDatepicker2Element, oldDatepicker2Element);
    }

    // Get the updated content (excluding datepicker2)
    var updatedContent = tempDiv.innerHTML;
    //test


    var inputValues = {};
    var checkboxStates = {};
    document.querySelectorAll('input').forEach(function (input) {
        if (input.type === 'text' || input.type === 'search') {
            inputValues[input.id] = input.value; // Store input field values
        }
        else if (input.type === 'checkbox') {
            checkboxStates[input.id] = input.checked; // Store checkbox states
        }
    });
    var savedData = {
        //content: content,
        content: updatedContent,
        inputValues: inputValues,
        checkboxStates: checkboxStates
    };
    localStorage.setItem('DateTimeFilterHTMLContent', JSON.stringify(savedData)); // Store the data
}

//#region Time JS
function initTimeFunction2() {
    var showTimeLists = document.querySelectorAll('.showTimeList2');
    showTimeLists.forEach(function (showTimeList) {
        showTimeList.addEventListener('click', function (event) {
            var timeList = showTimeList.nextElementSibling;
            if (timeList.style.display === "none" || timeList.style.display === "") {
                timeList.style.display = "block";
            } else {
                timeList.style.display = "none";
            }
            event.stopPropagation(); // Prevents the click event from bubbling up to the document
        });
    });

    document.addEventListener('click', function (event) {
        // Loop through each showTimeList
        showTimeLists.forEach(function (showTimeList) {
            var timeList = showTimeList.nextElementSibling;
            // Check if the click target is not the showTimeList, its timeList, or their descendants and close the timeList if it's open
            if (!showTimeList.contains(event.target) && !timeList.contains(event.target) && timeList.style
                .display === "block") {
                timeList.style.display = "none";
            }
        });
    });

    document.querySelectorAll('.timeList2').forEach(item => {
        item.addEventListener('click', event => {
            if (event.currentTarget.classList.contains('disabled')) {
                return;
            }
            document.querySelectorAll('.timeList2').forEach(item => {
                item.classList.remove('active');
            });
            event.currentTarget.classList.add('active');

            // Store the value if timeList has active class
            if (event.currentTarget.classList.contains('active')) {
                const timeValue = event.currentTarget.querySelector('.timeValue2').textContent;
                var selectedTimeDiv = document.getElementById("selected-time-id2");
                selectedTimeDiv.innerText = timeValue.trim();
                $('#main-time-id').html(timeValue.trim());
                const timeActualValue = event.currentTarget.querySelector('.timeValue2').getAttribute('data-info');
                $('#time-id2').val(timeActualValue.trim());
            }
        });
    });
}
//#endregion

//#region Time JS
function initPeopleFunction2() {
    var showPeopleLists = document.querySelectorAll('.showPeopleList2');
    showPeopleLists.forEach(function (showPeopleList) {
        showPeopleList.addEventListener('click', function (event) {
            var timeList = showPeopleList.nextElementSibling;
            if (timeList.style.display === "none" || timeList.style.display === "") {
                timeList.style.display = "block";
            } else {
                timeList.style.display = "none";
            }
            event.stopPropagation(); // Prevents the click event from bubbling up to the document
        });
    });

    document.addEventListener('click', function (event) {
        // Loop through each showTimeList
        showPeopleLists.forEach(function (showPeopleList) {
            var timeList = showPeopleList.nextElementSibling;
            // Check if the click target is not the showTimeList, its timeList, or their descendants and close the timeList if it's open
            if (!showPeopleList.contains(event.target) && !timeList.contains(event.target) && timeList.style
                .display === "block") {
                timeList.style.display = "none";
            }
        });
    });

    document.querySelectorAll('.peopleList2').forEach(item => {
        item.addEventListener('click', event => {
            document.querySelectorAll('.peopleList2').forEach(item => {
                item.classList.remove('active');
            });
            event.currentTarget.classList.add('active');
            // Store the value if timeList has active class
            if (event.currentTarget.classList.contains('active')) {
                const peopleValue = event.currentTarget.querySelector('.peopleValue2').textContent;
                var selectedPeopleDiv = document.getElementById("selected-noofpeople-id2");
                selectedPeopleDiv.innerText = peopleValue.trim();
                //var selectedPeopleDiv2 = document.getElementById("main-date-id");
                //selectedPeopleDiv2.innerText = peopleValue.trim();
                $('#main-noofpeople-id').html(peopleValue.trim());
                const peopleActualValue = event.currentTarget.querySelector('.peopleValue2').getAttribute('data-info');
                $('#noofpeople-id2').val(peopleActualValue.trim());
            }
        });
    });
}
//#endregion

function CloseInitiatedDateTimeFilterPopupFunction() {
    var element = document.getElementById('drawer-date-time');
    document.body.classList.remove('body-no-scroll');
    element.classList.add('translate-y-full');
    var removeElement = document.getElementById("ui-datepicker-div")
    removeElement.style.setProperty("display", "none", "important");
}

function SubmitDateTimeFilterFunction() {
    EnableLoaderFunction();
    //InitiateDateTimeFilterPopupFunction();
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    ManageDateTimeFilterHTMLContent();
    var form = document.getElementById("date-time-filter-id");
    form.action = "/search" + locationId;
    form.submit();
    DisableLoaderFunction();
}
//#endregion

function NewClubRenderFunction() {
    EnableLoaderFunction();
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    var searchUrl = `/Search${locationId}/?target=new&NewClub=${true}`;
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = searchUrl;
    document.body.appendChild(form);
    form.submit();
    DisableLoaderFunction();
}

function NewHostRenderFunction() {
    EnableLoaderFunction();
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    var searchUrl = `/Search${locationId}/?target=new&NewHost=${true}`;
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = searchUrl;
    document.body.appendChild(form);
    form.submit();
    DisableLoaderFunction();
}
function PopupFilterNewClubRenderFunction() {
    EnableLoaderFunction();
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    var searchUrl = `/Search${locationId}/?scftab=03&NewClub=${true}`;
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = searchUrl;
    document.body.appendChild(form);
    form.submit();
    DisableLoaderFunction();
}

function PopupFilterNewHostRenderFunction() {
    EnableLoaderFunction();
    let locationId = $('#filter-location-id').val();
    if (!locationId || locationId.trim() === '') {
        locationId = $('#current-location-id').val();
    }
    $('.location-class').val(locationId);
    var searchUrl = `/Search${locationId}/?scftab=03&NewHost=${true}`;
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = searchUrl;
    document.body.appendChild(form);
    form.submit();
    DisableLoaderFunction();
}
