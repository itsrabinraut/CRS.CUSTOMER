﻿using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.ReservationManagementV2;
using System;
using System.Collections.Generic;

namespace CRS.CUSTOMER.REPOSITORY.ReservationManagementV2
{
    public interface IReservationManagementV2Repository
    {
        #region InitiateClubReservationProcess
        InitiateClubReservationCommon InitiateClubReservationProcess(string ClubId, string SelectedDate = "");
        #endregion

        #region  Verify club and get club details
        Tuple<ResponseCode, string, ClubBasicDetailCommon> VerifyAndGetClubBasicDetail(string ClubId);
        #endregion

        #region check if the customer can proceed with the reservation process
        Tuple<ResponseCode, string> IsReservationProcessValid(string ClubId, string CustomerId, string SelectedDate, string SelectedTime, string NoOfPeople);
        #endregion

        #region Plan 
        Tuple<ResponseCode, string, List<PlanV2Common>> GetPlans(string ClubId, string CustomerId);
        #endregion

        #region Host
        List<HostListV2Common> GetHostList(string ClubId);
        #endregion

        #region Get host details by club and host id
        List<HostListV2Common> GetSelectedHostDetail(string ClubId, string HostListId);
        #endregion
    }
}
