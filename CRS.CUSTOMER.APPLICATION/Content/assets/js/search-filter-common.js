//#region LOCATION FILTER POPUP
function GetLocationFilterPopUp() {
    EnableLoaderFunction();
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
    if (element) {
        element.classList.add('translate-y-full');
        return false;
    }
}
//#endregion

//#region PREFERENCE FILTER POPUP
function GetPreferenceFilterPopUp() {
    EnableLoaderFunction();
    var preferencefilterpopupContent = $('#preferencefilterpopUp-id').html();
    if (preferencefilterpopupContent.trim() !== '') {
        var element = document.getElementById('drawer-filter-location');
        if (element) {
            element.classList.remove('translate-y-full');
            DisableLoaderFunction();
            return false;
        }
    }
    $.ajax({
        type: 'GET',
        async: true,
        url: '/DashboardV2/GetPreferenceFilterPopUp',
        dataType: 'json',
        data: {
        },
        success: function (data) {
            debugger;
            if (!data || data.Code !== 0) {
                toastr.info(data?.Message);
                DisableLoaderFunction();
                return false;
            }
            $('#preferencefilterpopUp-id').html(data.PartialView);

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
                    console.log("locationSelectValue", locationSelectValue)
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
            console.log(multiSelectValues);

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
                    console.log(multiSelectValues);
                    // Log the array of selected values
                });
            });
            //#endregion

            //#region 3
            // Define searchbybusinesshours
            var searchbybusinesshours = document.querySelectorAll('.search-by-business-hours');

            // Define searchbybusinesshoursinputField
            var searchbybusinesshoursinputField = document.querySelector('input[name="searchbybusinesshours"]');

            // Add event listener to each searchbybusinesshours div
            searchbybusinesshours.forEach(function (div) {
                div.addEventListener('click', function () {
                    // Remove the 'active' class from all divs
                    searchbybusinesshours.forEach(function (d) {
                        d.classList.remove('active');
                    });
                    // Add the 'active' class to the clicked div
                    this.classList.add('active');
                    if (this.classList.contains('contains-time-class')) {
                        var div = document.getElementById('time-div-id');
                        div.removeAttribute('hidden');
                    }
                    // Get the text content of the specific element within the clicked div
                    var searchbybusinesshoursSelectValue = this.querySelector('.searchbybusinesshoursValue')
                        .textContent.trim();
                    // Set the value of the input field to the content of the specific element
                    searchbybusinesshoursinputField.value = searchbybusinesshoursSelectValue;
                });
            });
            //#endregion

            //#region 4
            //#endregion

            //#region 5
            //debugger;
            //const tabs = document.querySelector(".wrapper");
            //const tabButton = document.querySelectorAll(".tab-button");
            //const contents = document.querySelectorAll(".content");

            //tabs.onclick = (e) => {
            //    const id = e.target.dataset.id;
            //    if (id) {
            //        tabButton.forEach((btn) => {
            //            btn.classList.remove("active");
            //        });
            //        e.target.classList.add("active");

            //        contents.forEach((content) => {
            //            content.classList.remove("active");
            //        });
            //        const element = document.getElementById(id);
            //        element.classList.add("active");
            //    }
            //};
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
                        debugger;
                        const timeValueElement = event.currentTarget.querySelector('.timeValue');
                        if (timeValueElement) {
                            var timeValue = timeValueElement.getAttribute('data-info');
                            var selectedTimeDiv = document.getElementById("selected-time-id");
                            selectedTimeDiv.innerText = timeValue.trim();
                            $('#time-id').val(timeValue.trim());
                        }
                    }
                });
            });
            //#endregion

            //#region 7
            function seemore() {
                var x = document.getElementById("seemore");

                if (x.style.display === "none") {
                    x.style.display = "grid";
                } else {
                    x.style.display = "none";
                }
            }



            const checkboxes = document.querySelectorAll('input[name="myCheckbox"]');

            checkboxes.forEach((checkbox) => {
                checkbox.addEventListener('change', function () {
                    if (this.checked) {
                        checkboxes.forEach((otherCheckbox) => {
                            if (otherCheckbox !== this) {
                                otherCheckbox.checked = false;
                            }
                        });
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

                    // Show the selected tab and set active class on the clicked button
                    tabs[index].classList.remove("hidden");
                    tabButton.classList.add("active");
                });
            });
            //#endregion
            DisableLoaderFunction();
        },
        error: function (xhr, status, error) {
            toastr.info("Something went wrong. Please try again later.");
            DisableLoaderFunction();
            return false;
        }
    });
}

function ClosePreferenceFilterPopUp() {
    var element = document.getElementById('drawer-filter-location');
    if (element) {
        element.classList.add('translate-y-full');
        return false;
    }
}
//#endregion

//#region 1
function LocationFunction(i) {
    window.location.href = "/LocationManagement/Index?LocationId=" + i;
}
//#endregion
