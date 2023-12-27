using AutoMapper;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Utilities
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            var config = new MapperConfiguration(cfg =>
            {
                //Configuring PostDischargeInfo to AddContactsRequest
                cfg.CreateMap<PostDischargeInfoPlusGenesys, AddContactsRequest>()
                    .ForMember(acr => acr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
                    .ForPath(acr => acr.Data.NhMemberId, opt => opt.MapFrom(src => src.NHMemberId))
                    .ForPath(acr => acr.Data.MemberName, opt => opt.MapFrom(src => src.MemberName))
                    .ForPath(acr => acr.Data.Language, opt => opt.MapFrom(src => src.Language))
                    .ForPath(acr => acr.Data.Address, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Address1) ? src.Address2 : src.Address1))
                    .ForPath(acr => acr.Data.Region, opt => opt.MapFrom(src => src.Region))
                    .ForPath(acr => acr.Data.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNbr))
                    .ForPath(acr => acr.Data.CarrierName, opt => opt.MapFrom(src => src.CarrierName))
                    .ForPath(acr => acr.Data.LoadDate, opt => opt.MapFrom(src => src.LoadDate))
                    .ForPath(acr => acr.Data.DischargeDate, opt => opt.MapFrom(src => src.DischargeDate))
                    .ForPath(acr => acr.Data.DayCount, opt => opt.MapFrom(src => src.DayCount))
                    .ForPath(acr => acr.Data.AttemptCountToday, opt => opt.MapFrom(src => src.AttemptCountToday))
                    .ForPath(acr => acr.Data.AttemptCountTotal, opt => opt.MapFrom(src => src.AttemptCountTotal));
                // UpdateContactsRequest
                cfg.CreateMap<PostDischargeInfoPlusGenesys, UpdateContactsRequest>()
                    .ForMember(ucr => ucr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
                    .ForPath(ucr => ucr.Data.NhMemberId, opt => opt.MapFrom(src => src.NHMemberId))
                    .ForPath(ucr => ucr.Data.MemberName, opt => opt.MapFrom(src => src.MemberName))
                    .ForPath(ucr => ucr.Data.Language, opt => opt.MapFrom(src => src.Language))
                    .ForPath(ucr => ucr.Data.Address, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Address1) ? src.Address2 : src.Address1))
                    .ForPath(ucr => ucr.Data.Region, opt => opt.MapFrom(src => src.Region))
                    .ForPath(ucr => ucr.Data.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNbr))
                    .ForPath(ucr => ucr.Data.CarrierName, opt => opt.MapFrom(src => src.CarrierName))
                    .ForPath(ucr => ucr.Data.LoadDate, opt => opt.MapFrom(src => src.LoadDate))
                    .ForPath(ucr => ucr.Data.DischargeDate, opt => opt.MapFrom(src => src.DischargeDate))
                    .ForPath(ucr => ucr.Data.DayCount, opt => opt.MapFrom(src => src.DayCount))
                    .ForPath(ucr => ucr.Data.AttemptCountToday, opt => opt.MapFrom(src => src.AttemptCountToday))
                    .ForPath(ucr => ucr.Data.AttemptCountTotal, opt => opt.MapFrom(src => src.AttemptCountTotal));
                cfg.CreateMap<GetContactsResponse, PostDischargeInfoPlusGenesys>()
                    .ForMember(ucr => ucr.PostDischargeId, opt => opt.MapFrom(src => src.Id))
                    .ForPath(ucr => ucr.NHMemberId, opt => opt.MapFrom(src => src.Data.NhMemberId))
                    .ForPath(ucr => ucr.MemberName, opt => opt.MapFrom(src => src.Data.MemberName))
                    //.ForPath(ucr => ucr.Language, opt => opt.MapFrom(src => src.Data.Language))
                    .ForPath(ucr => string.IsNullOrWhiteSpace(ucr.Address1) ? ucr.Address2 : ucr.Address1, opt => opt.MapFrom(src => src.Data.Address))
                    .ForPath(ucr => ucr.Region, opt => opt.MapFrom(src => src.Data.Region))
                    .ForPath(ucr => ucr.PhoneNbr, opt => opt.MapFrom(src => src.Data.PhoneNumber))
                    .ForPath(ucr => ucr.CarrierName, opt => opt.MapFrom(src => src.Data.CarrierName))
                    .ForPath(ucr => ucr.LoadDate, opt => opt.MapFrom(src => src.Data.LoadDate))
                    .ForPath(ucr => ucr.DischargeDate, opt => opt.MapFrom(src => src.Data.DischargeDate))
                    .ForPath(ucr => ucr.DayCount, opt => opt.MapFrom(src => src.Data.DayCount))
                    .ForPath(ucr => ucr.AttemptCountToday, opt => opt.MapFrom(src => src.Data.AttemptCountToday))
                    .ForPath(ucr => ucr.AttemptCountTotal, opt => opt.MapFrom(src => src.Data.AttemptCountTotal));
            });
            //Create an Instance of Mapper and return that Instance
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
