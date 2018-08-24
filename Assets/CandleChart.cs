﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
using Nanome.Core;

public class CandleChart : MonoBehaviour
{

    internal class monthData
    {
        public double open, close, high, low;
        public DateTime date;

        public monthData()
        {

        }
        public monthData(double open, double close, double high, double low, DateTime date)
        {
            this.open = open;
            this.close = close;
            this.high = high;
            this.close = close;
            this.date = date;
        }

        public double getOpen()
        {
            return this.open;
        }

        public double getClose()
        {
            return this.close;
        }

        public double getHigh()
        {
            return this.high;
        }

        public double getLow()
        {
            return this.low;
        }

        public DateTime getDate()
        {
            return this.date;
        }

        public void setDate(DateTime date)
        {
            this.date = date;
        }

        public void setOpen(double open)
        {
            this.open = open;
        }

        public void setClose(double close)
        {
            this.close = close;
        }

        public void setHigh(double high)
        {
            this.high = high;
            getHigh();
        }

        public void setLow(double low)
        {
            this.low = low;
        }

    }

    internal class dayData
    {
        string start, end;
        public dayData(string start, string end)
        {
            this.start = start;
            this.end = end;
        }

        public string getStart()
        {
            return start;
        }
        public string getEnd()
        {
            return end;
        }
    }

    private static CandleChart _instance;
    public static CandleChart GetInstance()
    {
        return _instance;
    }

    private string URL;
    public void SetURL(string newURL)
    {
        URL = newURL;
    }

    // Use this for initialization
    string output;
    StringBuilder builder = new StringBuilder();
    private List<string> times = new List<string>();
    private List<string> prices = new List<string>();
    private List<monthData> months = new List<monthData>();
    private List<monthData> day = new List<monthData>();
    private List<monthData> scaledMonths = new List<monthData>();

    private List<dayData> dayTimes = new List<dayData>();

    List<int> temp = new List<int>();
    public List<GameObject> pointList = new List<GameObject>();
    float[] scaledPrices;
    float[] monthlyPrices;

    string startTime = "null", lastTime, firstTime, secondTime;
    DateTime firstDay, secondDay;

    GameObject frameObj;
    LineRenderer frameLine;

    // The factor by which number of points are determined. 1 = full points = lag
    public int numPoints = 3;

    // Higher resolution will stretch out the graph, lower will compress it.
    float resolution = 25f;
    //public int numPoints = 10;
    public Material frameMaterial;
    public Transform bullish;
    public Transform boorish;
    public Transform line;

    public Transform dataPoint;

    private TextMesh dataContent;

    float max, min;

    void Awake()
    {
        _instance = this;
        //gameObject.SetActive(false);
    }

    void Start()
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        double currTime = Math.Round((double)(DateTime.UtcNow - epoch).TotalMilliseconds);
        // Debug.Log("CURRTIME: " + currTime);
        SetURL("https://graphs2.coinmarketcap.com/currencies/bitcoin/0/" + currTime + "/");
        updateGraph();
    }

    public void kill()
    {
        StopAllCoroutines();
        // Destroy(frameLine);
        times = new List<string>();
        prices = new List<string>();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        pointList = new List<GameObject>();
        builder = new StringBuilder();
    }

    public void updateGraph()
    {
        Async obj = Async.runInCoroutine(GetText);
        obj.onEvent("Done", parseData);
    }

    void testMethod(object tmp)
    {
        string text = ((StringBuilder)tmp).ToString();
        text = text.Substring(text.IndexOf("Aug 06, 2018"), 600);
        Debug.Log(text);
    }

    void testDays()
    {
        Async obj = Async.runInCoroutine(GetAllText);

        // count++;
        // for (int i = 0; i < dayTimes.Count; ++i)
        // {
        //     SetURL("https://graphs2.coinmarketcap.com/currencies/bitcoin/" + dayTimes[i].getStart() + "/" + dayTimes[i].getEnd() + "/");
        //     Async obj = Async.runInCoroutine(GetText);
        //     obj.onEvent("Days", printData);
        // }
    }

    void printData(object tmp)
    {
        Debug.Log(((StringBuilder)tmp).ToString().Substring(0, 10));
    }

    int count = 0;
    void getDayDates(object tmp)
    {
        if (startTime.Equals("null"))
        {
            string data = ((StringBuilder)tmp).ToString();
            int startIndex = data.IndexOf("price_usd");
            int endIndex = data.IndexOf("]]", startIndex);

            string first = data.Substring(startIndex + 14, 13);

            int currIndex = endIndex;
            while (data[currIndex] != '[')
            {
                currIndex--;
            }
            string last = data.Substring(currIndex + 1, 13);

            startTime = first;
            firstTime = first;
            lastTime = last;

            firstDay = convertFromEpoch(firstTime);
            secondDay = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0).AddHours(23);
            secondDay = secondDay.AddMinutes(59);
            secondDay = secondDay.AddSeconds(59);

            secondTime = convertFromDateTime(secondDay);

            dayTimes.Add(new dayData(firstTime, secondTime));
            SetURL("https://graphs2.coinmarketcap.com/currencies/bitcoin/" + firstTime + "/" + secondTime + "/");
            //updateGraph();
            getDayDates(tmp);
        }
        else
        {
            //lock(day);
            //parseDaysData(tmp);
            while (Convert.ToDouble(lastTime) - Convert.ToDouble(secondTime) > 0)
            {
                firstDay = convertFromEpoch(secondTime).AddSeconds(1);
                secondDay = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0).AddHours(23);
                secondDay = secondDay.AddMinutes(59);
                secondDay = secondDay.AddSeconds(59);

                secondTime = convertFromDateTime(secondDay);
                firstTime = convertFromDateTime(firstDay);

                dayTimes.Add(new dayData(firstTime, secondTime));

            }
            /* 
                        if (Convert.ToDouble(lastTime) - Convert.ToDouble(secondTime) > 0)
                        {
                            updateGraph();
                        }
                        else
                        { */
            Async obj = Async.runInCoroutine(GetAllText);
            // }


            /* 
                        SetURL("https://graphs2.coinmarketcap.com/currencies/bitcoin/" + firstTime + "/" + secondTime + "/");

                        if (Convert.ToDouble(lastTime) - Convert.ToDouble(secondTime) > 0)
                        {
                            builder = new StringBuilder();
                            tmp = new object();
                            updateGraph();
                        }
                        else
                        {
                            tempMethod();
                        }
             */
        }
    }

    void parseDaysData(object tmp)
    {
        string data = ((StringBuilder)tmp).ToString();
        int startIndex = data.IndexOf("price_usd");
        int endIndex = data.IndexOf("]]", startIndex);

        //Debug.Log ("start: " + startIndex + ", End: " + endIndex);
        data = data.Substring(startIndex + 13, endIndex - startIndex);
        fillDayData(data);

    }

    void fillDayData(string allData)
    {
        prices = new List<string>();
        times = new List<string>();

        //Debug.Log(allData);

        while (allData.IndexOf("]]") > 5)
        {
            int endIndex = allData.IndexOf(",");
            times.Add(allData.Substring(1, endIndex - 1));
            //Debug.Log("CURR TIME: " + convertFromEpoch(allData.Substring(1, endIndex - 1)));

            allData = allData.Substring(endIndex + 2, allData.Length - endIndex - 2);
            endIndex = allData.IndexOf("]");
            prices.Add(allData.Substring(0, endIndex));
            allData = allData.Substring(endIndex + 3, allData.Length - endIndex - 3);
        }

        monthData currDay = new monthData();
        currDay.setOpen(Convert.ToDouble(prices[0]));
        currDay.setClose(Convert.ToDouble(prices[prices.Count - 1]));

        double max = currDay.getOpen(), min = currDay.getOpen();
        for (int i = 0; i < prices.Count; i++)
        {
            double currPrice = Convert.ToDouble(prices[i]);

            if (currPrice > max)
                max = currPrice;
            currDay.setDate(convertFromEpoch(times[i]));
            if (currPrice < min)
                min = currPrice;
        }

        currDay.setHigh(max);
        currDay.setLow(min);


        day.Add(currDay);
        Debug.Log(currDay.getDate());
    }

    void tempMethod()
    {
        for (int i = 0; i < dayTimes.Count; i++)
        {
            Debug.Log("START: " + convertFromEpoch(dayTimes[i].getStart()) + ", END: " + convertFromEpoch(dayTimes[i].getEnd()));
            /*         Debug.Log("MONTH: " + day[i].getDate() + ", HIGH: " + day[i].getHigh() + ", LOW: " + day[i].getLow());
                    Debug.Log("MONTH: " + day[i].getDate() + ", OPEN: " + day[i].getOpen() + ", CLOSE: " + day[i].getClose()); */

        }
    }


    // Extracts the usd price time/price data from the text
    void parseData(object tmp)
    {
        //  Debug.Log("parseData");
        string data = ((StringBuilder)tmp).ToString();
        int startIndex = data.IndexOf("price_usd");
        int endIndex = data.IndexOf("]]", startIndex);

        //Debug.Log ("start: " + startIndex + ", End: " + endIndex);
        data = data.Substring(startIndex + 13, endIndex - startIndex);
        fillData(data);
        //Debug.Log (data);
    }

    // Separates the time and price into separate lists
    void fillData(string allData)
    {
        while (allData.IndexOf("]]") > 5)
        {
            int endIndex = allData.IndexOf(",");
            times.Add(allData.Substring(1, endIndex - 1));

            allData = allData.Substring(endIndex + 2, allData.Length - endIndex - 2);
            endIndex = allData.IndexOf("]");
            prices.Add(allData.Substring(0, endIndex));
            allData = allData.Substring(endIndex + 3, allData.Length - endIndex - 3);
        }

        //Debug.Log("Times: " + times.Count + ", Prices: " + prices.Count);
        //  Debug.Log("lastTime: " + times[times.Count - 1] + ", lastPrice: " + prices[prices.Count - 1]);

        makeGraph(times, prices);
    }

    void makeGraph(List<string> times, List<string> prices)
    {
        this.times = times;
        this.prices = prices;

        int numMonths = (int)Mathf.Ceil((float)getNumMonths(convertFromEpoch(times[0]), convertFromEpoch(times[times.Count - 1])) * 4.348f);
        //int numMonths = (int)Mathf.Ceil(times.Count / 7f);;
        Debug.Log("weeks: " + numMonths);
        //fillMonthData();
        fillWeekData();
        string[] Ys = prices.ToArray();
        Debug.Log("Here");
        rescaleData(Ys, 0);

        
        for (int i = 0; i < (numMonths - 1); i++)
        {
            float xPos = ((float)i) / ((numMonths - 1) / resolution) - 12.25f;
            float hiLowDiff = (float)scaledMonths[i].getHigh() - (float)scaledMonths[i].getLow();
            float barLen = (float)months[i].getHigh() - (float)months[i].getLow();
            float openCloseDiff = Math.Abs((float)months[i].getClose() - (float)months[i].getOpen());
            float topDiff, botDiff, botPercent, topPercent;
            float barPercent = openCloseDiff / ((float)months[i].getHigh() - (float)months[i].getLow());

            Transform barType;
            barType = months[i].getOpen() < months[i].getClose() ? bullish : boorish;


            // Green
            if (months[i].getOpen() < months[i].getClose())
            {
                topDiff = (float)months[i].getHigh() - (float)months[i].getClose();
                botDiff = (float)months[i].getOpen() - (float)months[i].getLow();
                botPercent = 1 - ((float)months[i].getLow() / (float)months[i].getOpen());
                topPercent = (float)months[i].getClose() / (float)months[i].getHigh();
            }
            // Red
            else
            {
                topDiff = (float)months[i].getHigh() - (float)months[i].getOpen();
                botDiff = (float)months[i].getClose() - (float)months[i].getLow();
                botPercent = 1 - ((float)months[i].getLow() / (float)months[i].getClose());
                topPercent = (float)months[i].getOpen() / (float)months[i].getHigh();
            }

/*             Debug.Log("MONTH: " + i + ", HIGH: " + months[i].getHigh() + ", LOW: " + months[i].getLow());
            Debug.Log("MONTH: " + i + ", OPEN: " + months[i].getOpen() + ", CLOSE: " + months[i].getClose()); */


            float lineScale = Math.Abs(hiLowDiff) + .5f;
            float height = ((float)scaledMonths[i].getHigh() + (float)scaledMonths[i].getLow()) / 2;

            height = height < 0 ? height - .5f : height + .25f;
            hiLowDiff = hiLowDiff < 0.5f ? hiLowDiff : hiLowDiff + .5f;

            //Debug.Log("MONTH: " + i + ", HEIGHT: " + height);
            //Debug.Log("MONTH: " + i + ", TOPDIFF: " + topDiff + ", BOTDIFF: " + botDiff + ", BARPOS: " + barPos);

            // Debug.Log("DIFF: " + hiLowDiff);

            GameObject stickHolderObj = new GameObject();
            stickHolderObj.name = "candleHolder" + i;

            Transform stickHolder = stickHolderObj.transform;
            stickHolder.parent = transform;
            stickHolder.localPosition = new Vector3(xPos, height, 0);
            stickHolder.localRotation = Quaternion.identity;
            // Debug.Log("MONTH: " + i + ", scaledPrice: " + monthlyPrices[i]);

            //Debug.Log("MONTH: " + i + ", POS: " + lineScale/2);
            Transform currLine = Instantiate(line, Vector3.zero, Quaternion.identity, stickHolder);
            //currLine.localPosition = new Vector3(xPos, scaledPrices[i], 0);
            currLine.localRotation = Quaternion.identity;
            currLine.localPosition = Vector3.zero;
            currLine.localScale += new Vector3(0, -1 + hiLowDiff, 0);

            //createDataPoint(stickHolder, "HIGH: " + months[i].getHigh(), new Vector3(.05f, 0, 0));

            float barSize = -1 + barPercent * hiLowDiff;
            // Debug.Log("MONTH: " + i + ", SIZE: " + barSize);
            float barPos = (.5f * barSize) * botDiff;

            float diffToUse = 0;

            Transform currBar = Instantiate(barType, Vector3.zero, Quaternion.identity, stickHolder);
            currBar.localScale += new Vector3(0, barSize, 0);

            if (botDiff == 0)
            {
                diffToUse = (-currLine.localScale.y / 2) + currBar.localScale.y / 2 - 0.01f;
            }
            else if (topDiff == 0)
            {
                diffToUse = currLine.localScale.y / 2 - currBar.localScale.y / 2 + 0.01f;
            }
            else
            {
                diffToUse = (-currLine.localScale.y / 2) + (currLine.localScale.y * (((topPercent + botPercent) / 2)));
            }

            currBar.localPosition = new Vector3(0, diffToUse, 0);
            currBar.localRotation = Quaternion.identity;

        }

        //Debug.Log("MIN: " + months[56].getLow());
        //Debug.Log("MIN SCALED: " + scaledMonths[56].getLow());
        //Debug.Log("MID SCALE: " + );

    }

    void createDataPoint(Transform parent, string text, Vector3 pos)
    {
        Transform currData = Instantiate(dataPoint, Vector3.zero, Quaternion.identity, parent);
        currData.localRotation = Quaternion.identity;
        currData.localPosition = pos;
        dataContent = currData.GetComponent<TextMesh>();
        dataContent.text = text;
    }

    void fillMonthData()
    {
        int numMonths = (int)getNumMonths(convertFromEpoch(times[0]), convertFromEpoch(times[times.Count - 1]));
        monthlyPrices = new float[numMonths];
        for (int i = 0; i < numMonths; ++i)
        {
            months.Add(new monthData());
            scaledMonths.Add(new monthData());
        }

        int currMonthNum = convertFromEpoch(times[0]).Month - 1;
        int currMonthIndex = -1;
        float monthlyTotal = 0;
        int dayCount = 0;

        for (int i = -1; i < times.Count - 1; ++i)
        {

            /*             if(currMonthIndex == 56){
                            Debug.Log("TIMESTAMP: " + times[i]);
                            Debug.Log("TIMES: " + convertFromEpoch(times[i]) + ", PRICES: " + prices[i]);
                        } */
            if (currMonthNum != convertFromEpoch(times[i + 1]).Month)
            {
                months[currMonthIndex + 1].setOpen(Convert.ToDouble(prices[i + 1]));
                months[currMonthIndex + 1].setClose(Convert.ToDouble(prices[i + 1]));
                months[currMonthIndex + 1].setHigh(Convert.ToDouble(prices[i + 1]));
                months[currMonthIndex + 1].setLow(Convert.ToDouble(prices[i + 1]));

                if (i != -1)
                {
                    months[currMonthIndex].setClose(Convert.ToDouble(prices[i]));
                    if (Convert.ToDouble(prices[i]) > months[currMonthIndex].getHigh())
                        months[currMonthIndex].setHigh(Convert.ToDouble(prices[i]));

                    if (Convert.ToDouble(prices[i]) < months[currMonthIndex].getLow())
                        months[currMonthIndex].setLow(Convert.ToDouble(prices[i]));

                    monthlyPrices[currMonthIndex] = monthlyTotal / dayCount;
                    dayCount = 0;
                    monthlyTotal = 0;
                }

                currMonthIndex++;

                if (currMonthNum == 12)
                    currMonthNum = 0;

                currMonthNum++;

            }
            else
            {
                // Checks for and sets a new high
                if (Convert.ToDouble(prices[i]) > months[currMonthIndex].getHigh())
                {
                    months[currMonthIndex].setHigh(Convert.ToDouble(prices[i]));
                }

                // Checks for and sets a new low
                if (Convert.ToDouble(prices[i]) < months[currMonthIndex].getLow())
                {
                    months[currMonthIndex].setLow(Convert.ToDouble(prices[i]));
                }

                monthlyTotal += (float)Convert.ToDouble(prices[i]);
                dayCount++;
            }

        }

        /* 
                for (int i = 0; i < numMonths; ++i)
                {
                    if(months[i].getHigh() < months[i].getClose() || months[i].getHigh() < months[i].getOpen() || months[i].getLow() > months[i].getClose() || months[i].getLow() > months[i].getOpen()){
                      Debug.Log("MONTH: " + i + ", HIGH: " + months[i].getHigh() + ", LOW: " + months[i].getLow());
                      Debug.Log("MONTH: " + i + ", OPEN: " + months[i].getOpen() + ", CLOSE: " + months[i].getClose());
                    }
                }
         */

    }

    void fillWeekData()
    {
        int numWeeks = (int)Mathf.Ceil((float)getNumMonths(convertFromEpoch(times[0]), convertFromEpoch(times[times.Count - 1])) * 4.348f);
        //int numWeeks = (int)Mathf.Ceil(times.Count / 7f) + 3;
        Debug.Log("num weeks: " + numWeeks);

        monthlyPrices = new float[numWeeks];
        for (int i = 0; i < numWeeks; ++i)
        {
            months.Add(new monthData());
            scaledMonths.Add(new monthData());
        }

        int currDayNum = (int)convertFromEpoch(times[0]).DayOfWeek;
        int currWeekIndex = -1;
        float monthlyTotal = 0;
        int dayCount = 0;

        int oldDay;

        oldDay = (int)convertFromEpoch(times[0]).Day;

        for (int i = -1; i < times.Count - 1; ++i)
        {
            

            Debug.Log("index: " + i + ", times: " + times.Count);
            Debug.Log("week: " + currWeekIndex);

            // Multiple timestamps from same day are counting as new weeks
            if (((int)convertFromEpoch(times[i + 1]).DayOfWeek == 0 && (int)convertFromEpoch(times[i+1]).Day != oldDay) || i == -1)
            {
                Debug.Log("New Week, " + currWeekIndex);
                months[currWeekIndex + 1].setOpen(Convert.ToDouble(prices[i + 1]));
                months[currWeekIndex + 1].setClose(Convert.ToDouble(prices[i + 1]));
                months[currWeekIndex + 1].setHigh(Convert.ToDouble(prices[i + 1]));
                months[currWeekIndex + 1].setLow(Convert.ToDouble(prices[i + 1]));

                if (i != -1)
                {
                    months[currWeekIndex].setClose(Convert.ToDouble(prices[i]));
                    if (Convert.ToDouble(prices[i]) > months[currWeekIndex].getHigh())
                        months[currWeekIndex].setHigh(Convert.ToDouble(prices[i]));

                    if (Convert.ToDouble(prices[i]) < months[currWeekIndex].getLow())
                        months[currWeekIndex].setLow(Convert.ToDouble(prices[i]));

                    monthlyPrices[currWeekIndex] = monthlyTotal / dayCount;
                    dayCount = 0;
                    monthlyTotal = 0;
                }

                currWeekIndex++;
            }
            else
            {
                // Checks for and sets a new high
                if (Convert.ToDouble(prices[i]) > months[currWeekIndex].getHigh())
                {
                    months[currWeekIndex].setHigh(Convert.ToDouble(prices[i]));
                }

                // Checks for and sets a new low
                if (Convert.ToDouble(prices[i]) < months[currWeekIndex].getLow())
                {
                    months[currWeekIndex].setLow(Convert.ToDouble(prices[i]));
                }

                monthlyTotal += (float)Convert.ToDouble(prices[i]);
                dayCount++;
            }

            oldDay = (int)convertFromEpoch(times[i+1]).Day;
        }

        Debug.Log("Here1");

    }

    // Rescales the y-values to fit between [-2,2] so that it can fit in the graph
    void rescaleData(string[] oldData, int fineTune)
    {
        scaledPrices = new float[oldData.Length];
        max = float.Parse(oldData[0]);
        min = float.Parse(oldData[0]);
        int minI = 0;
        for (int i = 0; i < oldData.Length; ++i)
        {

            if (float.Parse(oldData[i]) > max)
            {
                max = float.Parse(oldData[i]);
            }
            if (float.Parse(oldData[i]) < min)
            {
                min = float.Parse(oldData[i]);
                minI = i;
            }
        }

        float scale;

    // Debug.Log("SCALE: " + min / max);
    scalePrices:
        scale = findScale(min, max, fineTune);
        for (int i = 0; i < months.Count; ++i)
        {
            scaledMonths[i].setHigh((months[i].getHigh() / max * scale) - scale + 2);
            scaledMonths[i].setLow((months[i].getLow() / max * scale) - scale + 2);
            scaledMonths[i].setOpen((months[i].getOpen() / max * scale) - scale + 2);
            scaledMonths[i].setClose((months[i].getClose() / max * scale) - scale + 2);

            monthlyPrices[i] = (monthlyPrices[i] / max * scale) - scale + 2f;
            // Temporarily adjusts the scale if the data goes below the graph.
            if (scaledPrices[i] < -2.5)
            {
                fineTune -= 5;
                // goto scalePrices;
            }
        }
    }

    // The scale, min/max, represent how close together all of the points are.
    // As scale approaces 0, the points are well disbursed. As scale approaces 1, the points 
    // Are clumped together.
    // The closer to 1 the scale is, the more the points need to be stretched out.
    float findScale(float min, float max, int fineTune)
    {
        if (min / max > 0.9f)
        {
            return min / max * (70 + fineTune);
        }
        else if (min / max > 0.8f)
        {
            return min / max * (35 + fineTune);
        }
        else if (min / max > 0.6f)
        {
            return min / max * (20 + fineTune);
        }
        else if (min / max > 0.4f)
        {
            return min / max * (10 + fineTune);
        }
        else
        {
            return 4;
        }
    }

    public DateTime convertFromEpoch(string timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(timestamp));
    }

    public string convertFromDateTime(DateTime currDate)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan elapsed = currDate - epoch;
        return "" + elapsed.TotalMilliseconds;
    }

    public double getNumMonths(DateTime start, DateTime end)
    {
        return Math.Ceiling((end.Subtract(start)).Days / (double)30);
    }


    IEnumerator GetAllText(Async routine)
    {
        WWW[] wwws = new WWW[dayTimes.Count];
        for (int i = 0; i < dayTimes.Count; ++i)
        {
            SetURL("https://graphs2.coinmarketcap.com/currencies/bitcoin/" + dayTimes[i].getStart() + "/" + dayTimes[i].getEnd() + "/");
            wwws[i] = new WWW(URL);
        }
        Debug.Log("sentWWW");

        while (!AllDone(wwws))
        {
            yield return new WaitForSeconds(.1f);
        }

        Debug.Log("alldone");
    }

    bool AllDone(WWW[] wws)
    {
        bool AllDone = true;
        foreach (WWW www in wws)
        {
            AllDone &= www.isDone;
        }
        return AllDone;
    }



    IEnumerator GetText(Async routine)
    {
        using (WWW www = new WWW(URL))
        {
            /*             while(!www.isDone){
                            Thread.Sleep(1);
                        } */
            yield return www;
            yield return www.text;
            builder = new StringBuilder();
            builder.Append(www.text);
            routine.pushEvent("Done", builder);
        }
    }
}