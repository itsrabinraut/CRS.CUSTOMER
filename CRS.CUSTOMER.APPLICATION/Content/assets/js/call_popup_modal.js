
function callPopupModal(clubName, clubLogo, landlineNumber, locationName, type, clubId) {
    let html = '';
    if (type.trim().toUpperCase() === "B") {
        html = `<div id="call-now-popup-premium" 
                     class="fixed top-0 left-0 right-0 z-50 responsiveReviewModalPopup w-full p-4 px-[35px] h-full overflow-x-hidden overflow-y-hidden md:inset-0 h-[calc(100%-1rem)] max-h-full" style="background-color: #0D091D99; padding-left:35px; padding-right:35px">
                        <div class="relative w-full max-w-2xl max-h-full" style="top:25%;">
                            <span style="position:absolute; top:-24px; right:0px" class="cursor-pointer" onclick="closeCallNowPopup()">
                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 18 18" fill="none">
                                    <path d="M17.2929 2.51996C17.6834 2.12944 17.6834 1.49627 17.2929 1.10575L16.8943 0.707107C16.5037 0.316583 15.8706 0.316583 15.48 0.707107L9.70711 6.48004C9.31658 6.87056 8.68342 6.87056 8.29289 6.48004L2.51996 0.707107C2.12944 0.316583 1.49627 0.316583 1.10575 0.707107L0.707107 1.10575C0.316583 1.49627 0.316583 2.12944 0.707107 2.51996L6.48004 8.29289C6.87056 8.68342 6.87056 9.31658 6.48004 9.70711L0.707107 15.48C0.316583 15.8706 0.316583 16.5037 0.707107 16.8943L1.10575 17.2929C1.49627 17.6834 2.12944 17.6834 2.51996 17.2929L8.29289 11.52C8.68342 11.1294 9.31658 11.1294 9.70711 11.52L15.48 17.2929C15.8706 17.6834 16.5037 17.6834 16.8943 17.2929L17.2929 16.8943C17.6834 16.5037 17.6834 15.8706 17.2929 15.48L11.52 9.70711C11.1294 9.31658 11.1294 8.68342 11.52 8.29289L17.2929 2.51996Z" fill="white" />
                                    <path d="M16.9393 1.4593C17.1346 1.65457 17.1346 1.97115 16.9393 2.16641L11.1664 7.93934C10.5806 8.52513 10.5806 9.47487 11.1664 10.0607L16.9393 15.8336C17.1346 16.0289 17.1346 16.3454 16.9393 16.5407L16.5407 16.9393C16.3454 17.1346 16.0289 17.1346 15.8336 16.9393L10.0607 11.1664C9.47487 10.5806 8.52513 10.5806 7.93934 11.1664L2.16641 16.9393C1.97115 17.1346 1.65457 17.1346 1.4593 16.9393L1.06066 16.5407C0.865398 16.3454 0.865398 16.0289 1.06066 15.8336L6.83359 10.0607C7.41938 9.47487 7.41938 8.52513 6.83359 7.93934L1.06066 2.16641C0.865398 1.97115 0.865398 1.65457 1.06066 1.4593L1.4593 1.06066C1.65457 0.865398 1.97115 0.865398 2.16641 1.06066L7.93934 6.83359C8.52513 7.41938 9.47487 7.41938 10.0607 6.83359L15.8336 1.06066C16.0289 0.865398 16.3454 0.865398 16.5407 1.06066L16.9393 1.4593Z" stroke="white" stroke-opacity="0.9" />
                                </svg>
                            </span>
                            <div class="relative bg-white shadow dark:bg-gray-700 rounded-[6px]">
                                <img src="${clubLogo}" alt="img"
                                     class="h-[66px] w-[66px] rounded-full ring-2 ring-white absolute top-[-33px] left-[50%] translate-x-[-50%]" id="image" />
                                <div class="bg-white rounded-[6px]" style="padding-top:38px;">
                                    <div class="grid justify-center text-center pb-[13px]" style="padding-bottom:13px">
                                        <div class="text-[10px] text-warmgray-1 flex  justify-center items-center" id="location" style="gap:2px" style="padding-bottom:13px">
                                           
                                            <span><svg xmlns="http://www.w3.org/2000/svg" width="11" height="10" viewBox="0 0 11 10" fill="none">
  <path fill-rule="evenodd" clip-rule="evenodd" d="M2.6875 3.75C2.6875 2.20093 3.95093 0.9375 5.5 0.9375C7.04907 0.9375 8.3125 2.20093 8.3125 3.75C8.3125 4.18945 8.13428 4.6936 7.89258 5.24414C7.65088 5.79468 7.3396 6.37939 7.02344 6.92383C6.39111 8.01392 5.75391 8.92578 5.75391 8.92578L5.5 9.29688L5.24609 8.92578C5.24609 8.92578 4.60889 8.01392 3.97656 6.92383C3.6604 6.37939 3.34912 5.79468 3.10742 5.24414C2.86572 4.6936 2.6875 4.18945 2.6875 3.75ZM5.5 3.125C5.15454 3.125 4.875 3.40454 4.875 3.75C4.875 4.09546 5.15454 4.375 5.5 4.375C5.84546 4.375 6.125 4.09546 6.125 3.75C6.125 3.40454 5.84546 3.125 5.5 3.125Z" fill="#D75A8B"/>
</svg></span>
                                            ${locationName}
                                        </div>
                                        <div class="font-bold" id="clubnameenglish">${clubName}</div>
                                    </div>
                                    <div class="text-[14px] w-full flex justify-center items-center text-[#D75A8B] flex flex-col font-bold py-[10px]" style="background: #FBEAF3" id="vistidate">
                                        <span>ホスログを見たと </span><span>お伝えください</span>
                                        <div class="text-[#666] text-[10px] font-[500] flex justify-center items-center flex-col mt-[3px]"  >
                                            <span>初回料金・内容については店舗に直接</span>
                                            <span>お問い合わせください。</span>
                                        </div>
                                    </div>
                                    <div class="text-[18px] text-[#282828] text-center font-bold" style="padding-top:12px; padding-bottom:7px">${landlineNumber}</div>
                                    <div class="px-[20px]">
                                        <a class="btn-gradient-rounded w-full bg-[#D75A8B]" style="border-radius: 48px; background:#D75A8B;font-size:14px; font-weight:700" id="notification_url" href="tel:${landlineNumber}">電話予約する</a>
                                    </div>
                                    <div style="padding-top:16px; flex-direction:column; padding-bottom:7px; line-height:130%; color:#666" class="text-[#666] text-[12px] font-normal flex justify-center items-center " >
                                        <p>電話でのご予約はポイント</p>
                                        <p>対象外となりますのでご注意ください。</p>
                                    </div>
                                    <div style="padding-bottom:28px"><a style="text-align:center; line-height:130%; color:#888" class="text-[#888] text-[12px] font-[600] flex justify-center">ネット予約でポイントを獲得する</a></div>
                                </div>
                            </div>
                        </div>
                    </div>
                  `
    } else if (type.trim().toUpperCase() === "P") {
        html = `<div id="call-now-popup-premium"
                     class="fixed  top-0 left-0 block right-0  z-50 responsiveReviewModalPopup hidden w-full p-4 h-full overflow-x-hidden overflow-y-hidden md:inset-0 h-[calc(100%-1rem)] max-h-full" style="background-color: #0D091D99; overflow-y:hidden; display:block; padding-left:35px; padding-right:35px">
                    <div class="relative w-full max-w-2xl max-h-full" style="top:25%;">
                        <span style="position:absolute; top:-24px; right:0px" class="cursor-pointer" onclick="closeCallNowPopup()">
                            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 18 18" fill="none">
                                <path d="M17.2929 2.51996C17.6834 2.12944 17.6834 1.49627 17.2929 1.10575L16.8943 0.707107C16.5037 0.316583 15.8706 0.316583 15.48 0.707107L9.70711 6.48004C9.31658 6.87056 8.68342 6.87056 8.29289 6.48004L2.51996 0.707107C2.12944 0.316583 1.49627 0.316583 1.10575 0.707107L0.707107 1.10575C0.316583 1.49627 0.316583 2.12944 0.707107 2.51996L6.48004 8.29289C6.87056 8.68342 6.87056 9.31658 6.48004 9.70711L0.707107 15.48C0.316583 15.8706 0.316583 16.5037 0.707107 16.8943L1.10575 17.2929C1.49627 17.6834 2.12944 17.6834 2.51996 17.2929L8.29289 11.52C8.68342 11.1294 9.31658 11.1294 9.70711 11.52L15.48 17.2929C15.8706 17.6834 16.5037 17.6834 16.8943 17.2929L17.2929 16.8943C17.6834 16.5037 17.6834 15.8706 17.2929 15.48L11.52 9.70711C11.1294 9.31658 11.1294 8.68342 11.52 8.29289L17.2929 2.51996Z" fill="white" />
                                <path d="M16.9393 1.4593C17.1346 1.65457 17.1346 1.97115 16.9393 2.16641L11.1664 7.93934C10.5806 8.52513 10.5806 9.47487 11.1664 10.0607L16.9393 15.8336C17.1346 16.0289 17.1346 16.3454 16.9393 16.5407L16.5407 16.9393C16.3454 17.1346 16.0289 17.1346 15.8336 16.9393L10.0607 11.1664C9.47487 10.5806 8.52513 10.5806 7.93934 11.1664L2.16641 16.9393C1.97115 17.1346 1.65457 17.1346 1.4593 16.9393L1.06066 16.5407C0.865398 16.3454 0.865398 16.0289 1.06066 15.8336L6.83359 10.0607C7.41938 9.47487 7.41938 8.52513 6.83359 7.93934L1.06066 2.16641C0.865398 1.97115 0.865398 1.65457 1.06066 1.4593L1.4593 1.06066C1.65457 0.865398 1.97115 0.865398 2.16641 1.06066L7.93934 6.83359C8.52513 7.41938 9.47487 7.41938 10.0607 6.83359L15.8336 1.06066C16.0289 0.865398 16.3454 0.865398 16.5407 1.06066L16.9393 1.4593Z" stroke="white" stroke-opacity="0.9" />
                            </svg>
                        </span>
                        <!-- Modal content -->
                        <div class="relative bg-white  shadow dark:bg-gray-700 rounded-[6px]">
                            <img src="${clubLogo}" alt="img"
                                 class="h-[66px] w-[66px] rounded-full ring-2 ring-white absolute top-[-33px] left-[50%] translate-x-[-50%]" id="image" />
                            <!-- Modal body -->
                            <input type="hidden" value="" id="notification_modal">
                            <input type="hidden" value="" id="notification_image">
                            <div class=" bg-white rounded-[6px]" style="padding-top:38px;">
                                <div class="grid justify-center text-center pb-[13px]" style="padding-bottom:13px">
                                    <div class="text-[10px] text-warmgray-1 flex gap-1 justify-center" id="location">
                                        <i class="fa-solid fa-location-dot text-primaryDark"></i>${locationName}
                                    </div>
                                    <div class="font-bold" id="clubnameenglish">${clubName}</div>
                                </div>
                                <div class="text-[14px]  w-full flex justify-center items-center text-[#D75A8B] flex flex-col font-bold py-[10px]" style="background: #FBEAF3" id="vistidate"> <span>ホスログを見たと </span><span>お伝えください</span>  </div>
                                <div class="text-[18px] text-[#282828] text-center font-bold" style="padding-top:12px; padding-bottom:7px">${landlineNumber}</div>

                                <div class=" px-[20px]">
                                    <a class="btn-gradient-rounded w-full bg-[#D75A8B]" style="border-radius: 48px; background: #D75A8B; font-size:14px; font-weight:700" id="notification_url" href='tel:${landlineNumber}'>電話予約する</a>
                                </div>
                                <div style="padding-top:16px; padding-bottom:7px; line-height:130%;color:#666" class="text-[#666] text-[12px] font-normal flex flex-col justify-center items-center">
                                    <span>電話でのご予約はポイント</span>
                                    <span>対象外となりますのでご注意ください。</span>
                                </div>
                                <div class="" style="padding-bottom:28px"><a style="text-align:center;line-height:130%" class="text-[#D75A8B] text-[12px] underline font-[600] flex justify-center" onclick="bookNowPopup('${clubId}')">ネット予約でポイントを獲得する</a></div>
                            </div>
                        </div>
                    </div>
                </div>
              `
    };
    document.body.insertAdjacentHTML('beforeend', html);
    document.body.classList.add('scroll-popup-scroll-none');
    const scrollY = window.scrollY;
    document.body.style.position = 'fixed';
    document.body.style.top = `-${scrollY}px`;
    document.body.style.width = '100%';
    document.getElementById('club-bottom-tab-id').style.display = 'none'
}
function closeCallNowPopup() {
    const modal = document.getElementById('call-now-popup-premium');
    if (modal) {
        document.body.classList.remove('scroll-popup-scroll-none');
        const scrollY = document.body.style.top; // Retrieve the top position 
        document.body.style.position = '';
        document.body.style.top = '';
        window.scrollTo(0, parseInt(scrollY || '0') * -1); // Restore scroll position 
        modal.remove();
    }
    document.getElementById('club-bottom-tab-id').style.display = 'none'
}

function bookNowPopup(clubId) {
    InitiateClubReservationFunction(clubId)
    closeCallNowPopup()
    document.getElementById('club-bottom-tab-id').style.display = 'none'
}

