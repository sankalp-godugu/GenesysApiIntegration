using AutoMapper;
using MemberContactInfoJob.Model.Request;
using MemberContactInfoJob.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberContactInfoJob.Utility
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            var config = new MapperConfiguration(cfg =>
            {
                //Configuring PostDischargeInfo to AddContactsRequest
                cfg.CreateMap<PostDischargeGenesysInfo, AddContactsRequest>()
                    .ForPath(acr => acr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
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
                //Any Other Mapping Configuration ....
                cfg.CreateMap<PostDischargeGenesysInfo, UpdateContactsRequest>()
                    .ForPath(acr => acr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
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
            });
            //Create an Instance of Mapper and return that Instance
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
