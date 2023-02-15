// Copyright (c) 2023 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Globalization;
using Microsoft.MixedReality.Toolkit.Data;
using Newtonsoft.Json;
using UnityEngine;

public class OpenWeatherDataSourceGOJson : DataSourceGOJson
{
    private const string OpenWeatherUrl = "https://api.openweathermap.org/data/2.5/weather?units=metric";


    [Header("OpenWeather Settings")]
    public string Appid = "";

    public string Lat = "";

    public string Lon = "";

    [SerializeField]
    [Header("Debugging")]
    [Tooltip("The Open Weather Response JSON payload.")]
    private TextAsset DummyData;
    
    private void Start()
    {
        StartCoroutine(RequestOpenWeather());
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private IEnumerator RequestOpenWeather()
    {
        var result = DummyData.text;

        if (!string.IsNullOrEmpty(Appid))
        {
            var uri = OpenWeatherUrl + $"&lat={Lat}&lon={Lon}&appid={Appid}";
            yield return StartJsonRequest(uri, (json, obj) => result = json);
        }

        var deserializeObject = JsonConvert.DeserializeObject<Rootobject>(result);
        deserializeObject.wind.deg = WindDirect(Convert.ToDouble(deserializeObject.wind.deg));
        deserializeObject.dt = ConvertDateTimeFormat(deserializeObject.dt, deserializeObject.timezone);
        deserializeObject.sys.sunrise =
            ConvertDateTimeFormat(deserializeObject.sys.sunrise, deserializeObject.timezone);
        deserializeObject.sys.sunset = ConvertDateTimeFormat(deserializeObject.sys.sunset, deserializeObject.timezone);
        deserializeObject.visibility = Math.Floor(deserializeObject.visibility / 100f) / 10d;

        var sre = JsonConvert.SerializeObject(deserializeObject);
        DataSourceJson.UpdateFromJson(sre);
    }

    private static string ConvertDateTimeFormat(string UnixTimeSeconds, int TimezoneSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(UnixTimeSeconds))
            .ToOffset(TimeSpan.FromSeconds(TimezoneSeconds)).ToString("f", new CultureInfo("en-US"));
    }

    public string WindDirect(double d)
    {
        if (d <= 11)
            return " N ";
        if (d <= 33)
            return "NNE";
        if (d <= 56)
            return " NE ";
        if (d <= 78)
            return "ENE";
        if (d <= 101)
            return " E ";
        if (d <= 123)
            return "ESE";
        if (d <= 146)
            return " SE ";
        if (d <= 167)
            return "SSE";
        if (d <= 191)
            return " S ";
        if (d <= 213)
            return "SSW";
        if (d <= 236)
            return " SW ";
        if (d <= 258)
            return "WSW";
        if (d <= 281)
            return " W ";
        if (d <= 303)
            return "WNW";
        if (d <= 326)
            return " NW ";
        if (d <= 348)
            return "NNW";
        return " N ";
    }

    public class Rootobject
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public double visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public string dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public string deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
}