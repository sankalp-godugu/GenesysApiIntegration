using AutoMapper;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;

namespace GenesysContactsProcessJob.Utilities
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            MapperConfiguration config = new(cfg =>
            {
                //Configuring PostDischargeInfo to AddContactsRequest
                _ = cfg.CreateMap<PostDischargeInfo_GenesysMemberContactInfo, AddContactsRequest>()
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
                _ = cfg.CreateMap<PostDischargeInfo_GenesysMemberContactInfo, UpdateContactsRequest>()
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

                _ = cfg.CreateMap<GetContactsResponse, PostDischargeInfo_GenesysMemberContactInfo>()
                    .ForMember(gcr => gcr.PostDischargeId, opt => opt.MapFrom(src => src.Id))
                    .ForPath(gcr => gcr.NHMemberId, opt => opt.MapFrom(src => src.Data.NhMemberId))
                    .ForPath(gcr => gcr.MemberName, opt => opt.MapFrom(src => src.Data.MemberName))
                    //.ForPath(ucr => ucr.Language, opt => opt.MapFrom(src => src.Data.Language))
                    //.ForPath(gcr => string.IsNullOrWhiteSpace(gcr.Address1) ? gcr.Address2 : gcr.Address1, opt => opt.MapFrom(src => src.Data.Address))
                    .ForPath(gcr => gcr.Region, opt => opt.MapFrom(src => src.Data.Region))
                    .ForPath(gcr => gcr.PhoneNbr, opt => opt.MapFrom(src => src.Data.PhoneNumber))
                    .ForPath(gcr => gcr.CarrierName, opt => opt.MapFrom(src => src.Data.CarrierName))
                    .ForPath(gcr => gcr.LoadDate, opt => opt.MapFrom(src => src.Data.LoadDate))
                    .ForPath(gcr => gcr.DischargeDate, opt => opt.MapFrom(src => src.Data.DischargeDate))
                    .ForPath(gcr => gcr.DayCount, opt => opt.MapFrom(src => src.Data.DayCount))
                    .ForPath(gcr => gcr.AttemptCountToday, opt => opt.MapFrom(src => src.Data.AttemptCountToday))
                    .ForPath(gcr => gcr.AttemptCountTotal, opt => opt.MapFrom(src => src.Data.AttemptCountTotal));
            });

            //Create an Instance of Mapper and return that Instance
            Mapper mapper = new(config);
            return mapper;
        }
    }
}
