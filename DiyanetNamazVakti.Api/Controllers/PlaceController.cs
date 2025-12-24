using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Linq;
using DiyanetNamazVakti.Api.Core.ValueObjects;

namespace DiyanetNamazVakti.Api.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlaceController : ControllerBase
{
    private readonly IPlaceService _placeService;

    public PlaceController(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    [HttpGet("Countries")]
    public async Task<ActionResult<IResult>> Country()
    {
        try
        {
            var countries = await _placeService.GetCountries();
            if (countries == null)
            {
                return new ErrorDataResult<List<IdCodeName<int>>>("Ülke listesi alınamadı. Diyanet API'ye ulaşılamıyor olabilir.");
            }
            return new SuccessDataResult<List<IdCodeName<int>>>(countries);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("Kota"))
        {
            // Rate limit hatası - mesajı parse et
            var errorMessage = ex.Message.Contains(":") ? ex.Message.Split(':').Last().Trim() : "Kota aşıldı";
            return new ErrorDataResult<List<IdCodeName<int>>>(errorMessage);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<List<IdCodeName<int>>>($"Hata: {ex.Message}");
        }
    }

    [HttpGet("States")]
    public async Task<ActionResult<IResult>> GetAllStates()
    {
        return new SuccessDataResult<List<IdCodeName<int>>>(await _placeService.GetStates());
    }

    [HttpGet("States/{countryId}")]
    public async Task<ActionResult<IResult>> State(int countryId)
    {
        return new SuccessDataResult<List<IdCodeName<int>>>(await _placeService.GetStatesByCountry(countryId));
    }

    [HttpGet("Cities")]
    public async Task<ActionResult<IResult>> GetAllCities()
    {
        return new SuccessDataResult<List<IdCodeName<int>>>(await _placeService.GetCities());
    }

    [HttpGet("Cities/{stateId}")]
    public async Task<ActionResult<IResult>> City(int stateId)
    {
        return new SuccessDataResult<List<IdCodeName<int>>>(await _placeService.GetCitiesByState(stateId));
    }

    [HttpGet("CityDetail/{cityId}")]
    public async Task<ActionResult<IResult>> CityDetail(int cityId)
    {
        return new SuccessDataResult<CityDetailModel>(await _placeService.GetCity(cityId));
    }
}
