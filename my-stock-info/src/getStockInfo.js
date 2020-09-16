import axios from 'axios'

var url_server = "http://114.33.59.86:5000/";

function option() {
    this.type = {
        title: {
            text: '個股單價',
            x: 'center',
            align: 'right',
            y: '15px'
        },
        xAxis: {
            type: 'category',
            data: []
        },
        yAxis: {
            name: '單價:元(NT)',
            type: 'value',
            min: 0, //最小
            max: 500, //最大
        },
        tooltip: { //提示
            trigger: 'axis'
        },
        series: [{
            data: [],
            type: 'line'
        }]
    };
};

export function getDate(list_type) {

    //抓取當天日期
    var s_time_src = new Date();
    var e_time_src = new Date();
    var year;
    var month;
    var day;

    //依據類型扣年月
    switch (list_type) {
        case "1_month":
            s_time_src.setMonth(s_time_src.getMonth() - 1);
            break;
        case "3_month":
            s_time_src.setMonth(s_time_src.getMonth() - 3);
            break;
        case "6_mounth":
            s_time_src.setMonth(s_time_src.getMonth() - 6);
            break;
        case "1_year":
            console.log("sTime1" + s_time_src);
            console.log("s_time_src.getYear():" + s_time_src.getFullYear());
            s_time_src.setYear(s_time_src.getFullYear() - 1);
            console.log("sTime2" + s_time_src);
            break;
        case "3_year":
            s_time_src.setYear(s_time_src.getFullYear() - 3);
            break;
        case "all_year":
            s_time_src.setYear(s_time_src.getFullYear() - 100);
            break;
        default:
            break;
    }

    //取年
    year = e_time_src.getFullYear()
    //取月
    if ((e_time_src.getMonth() + 1) < 10)
        month = "0" + (e_time_src.getMonth() + 1)
    else
        month = "" + (e_time_src.getMonth() + 1)
    //取日
    if (e_time_src.getDate() < 10)
        day = "0" + e_time_src.getDate()
    else
        day = "" + e_time_src.getDate()

    var e_time = year + "-" + month + "-" + day

    //取年
    year = s_time_src.getFullYear()
    //取月
    if ((s_time_src.getMonth() + 1) < 10)
        month = "0" + (s_time_src.getMonth() + 1)
    else
        month = "" + (s_time_src.getMonth() + 1)
    //取日
    if (s_time_src.getDate() < 10)
        day = "0" + s_time_src.getDate()
    else
        day = "" + s_time_src.getDate()

    //取得時間區間起始
    var s_time = year + "-" + month + "-" + day

    var time = { start: s_time, end: e_time };

    return time;

}

//取得價格紀錄
export async function getStockPrices(stock_no, s_time, e_time, type) {

    //刷新價格資訊
    var url = url_server + "getRangePrices/" + stock_no + "/" + s_time + "/" + e_time + "/" + type;
    console.log("url:" + url)

    //instance.get('/getRangePrices/2330/2018-07-01/2018-07-31')
    var new_otion = new option();
    var new_list = new_otion.type;
    new_list.title.text = '個股單價';
    new_list.yAxis.name = '單價:元(NT)';

    //console.log( 'log0:'+JSON.stringify(otion));
    console.log('log1:' + JSON.stringify(new_list));
    try {
        console.log('test')
        const response = await axios(url);
        var data = response.data;
        //console.log('data' +  JSON.stringify(data))
        //console.log('response' + JSON.stringify(response))
        //var max = 0, min = 9999;
        var tmp_array = [];
        for (var i = 0; i < data.length; i++) {
            //console.log(data[i].date);
            //console.log(data[i].price);
            new_list.xAxis.data[i] = data[i].date;
            new_list.series[0].data[i] = data[i].price;
            if (data[i].price !== undefined)
                tmp_array[i] = data[i].price;
        }

        new_list.yAxis.max = Math.floor(Math.max.apply(null, tmp_array) * 1.1); //求最大值 
        new_list.yAxis.min = Math.ceil(Math.min.apply(null, tmp_array) * 0.9); //求最小值
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }
    // console.log( 'log2:'+JSON.stringify(new_list));
    return JSON.stringify(new_list);
}

//取得交易量紀錄
export async function getTradingVolume(stock_no, s_time, e_time) {
    var url = url_server + "getTraceAmount/" + stock_no + "/" + s_time + "/" + e_time;
    console.log("url:" + url)

    //instance.get('/getTraceAmount/2330/2018-07-01/2018-07-31')
    var new_otion = new option();
    var new_list = new_otion.type; //初始化 
    new_list.title.text = '交易量';
    new_list.yAxis.name = '數量:張';
    try {
        console.log('test')
        const response = await axios(url);
        var data = response.data.data;

        var tmp_array = [];
        for (var i = 0; i < data.length; i++) {
            new_list.xAxis.data[i] = data[i].date;
            new_list.series[0].data[i] = data[i].amount;
            tmp_array[i] = data[i].amount;
        }

        var max = Math.max.apply(null, tmp_array); //求最大值 
        var min = Math.min.apply(null, tmp_array); //求最小值

        new_list.yAxis.max = max * 1.2;
        new_list.yAxis.min = min * 0.8;

    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }


    return JSON.stringify(new_list);
}

//取得EPS紀錄
export async function getEPS(stock_no) {

    //刷新價格資訊
    var url = url_server + "getEPS/" + stock_no;
    console.log("url:" + url)

    //instance.get('/getEPS/2330')
    var new_otion = new option();
    var new_list = new_otion.type;
    new_list.title.text = '每季EPS';
    new_list.yAxis.name = '元(NT)';

    //console.log( 'log0:'+JSON.stringify(otion));
    console.log('log1:' + JSON.stringify(new_list));
    try {
        console.log('test')
        const response = await axios(url);
        var data = response.data;
        //console.log('data' +  JSON.stringify(data))
        //console.log('response' + JSON.stringify(response))
        //var max = 0, min = 9999;
        var tmp_array = [];
        new_list.series[0].type = 'bar';
        for (var i = 0; i < data.length; i++) {
            //console.log(data[i].date);
            //console.log(data[i].price);
            new_list.xAxis.data[i] = data[i].year + "-" + data[i].season;
            new_list.series[0].data[i] = data[i].eps;
            if (data[i].price !== undefined)
                tmp_array[i] = data[i].eps;
        }

        new_list.yAxis.max = Math.floor(Math.max.apply(null, tmp_array) * 1.1); //求最大值 
        new_list.yAxis.min = Math.ceil(Math.min.apply(null, tmp_array) * 0.9); //求最小值
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }
    // console.log( 'log2:'+JSON.stringify(new_list));
    return JSON.stringify(new_list);
}

//取得月營收紀錄
export async function getOperatingIncomes(stock_no) {

    //刷新價格資訊
    var url = url_server + "getOperatingIncomes/" + stock_no;
    console.log("url:" + url)

    var new_otion = new option();
    var new_list = new_otion.type;
    console.log('log1:' + JSON.stringify(new_list));

    try {
        const response = await axios(url);
        var data = response.data;
        var tableList = [];
        for (var i = 0; i < data.length; i++) {
            tableList[i] =
                createTable(data[i].opme002, data[i].opme003, data[i].opme004, data[i].opme005, data[i].opme006 + "%", data[i].opme007 + "%", data[i].opme008, data[i].opme009, data[i].opme010 + "%")
        }
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }

    //console.log('log2:' + JSON.stringify(tableList));
    return tableList;
}

//取得股利紀錄
export async function getDividend(stock_no) {

    //刷新價格資訊
    var url = url_server + "getDividend/" + stock_no;
    console.log("url:" + url)

    var new_otion = new option();
    var new_list = new_otion.type;
    console.log('log1:' + JSON.stringify(new_list));

    try {
        const response = await axios(url);
        var data = response.data;
        var tableList = [];
        for (var i = 0; i < data.length; i++) {
            tableList[i] =
                createTable(data[i].divd002, data[i].divd003, data[i].divd004, data[i].divd005, data[i].divd006, data[i].divd007, data[i].divd008, data[i].divd009 + "%", data[i].divd010 + "%")
        }
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }

    console.log('log2:' + JSON.stringify(tableList));
    return tableList;
}

//取得利潤紀錄
export async function getProfit(stock_no) {

    //刷新價格資訊
    var url = url_server + "getProfit/" + stock_no;
    console.log("url:" + url)

    var new_otion = new option();
    var new_list = new_otion.type;
    console.log('log1:' + JSON.stringify(new_list));

    try {
        const response = await axios(url);
        var data = response.data;
        var tableList = [];
        for (var i = 0; i < data.length; i++) {
            tableList[i] =
                createTable(data[i].prft002, data[i].prft003, data[i].prft004, data[i].prft005, data[i].prft006, data[i].prft007, data[i].prft008, data[i].prft009)
        }
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }

    console.log('log2:' + JSON.stringify(tableList));
    return tableList;
}

//取得評估結果
export async function getEvaluations(stock_no) {

    //刷新價格資訊
    var url = url_server + "getEvaluation/" + stock_no;
    console.log("url:" + url)
    var data;

    try {
        const response = await axios(url);
        data = response.data;
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }

    return data;
}

function createTable(column0, column1, column2, column3, column4, column5, column6, column7, column8) {
    return { column0, column1, column2, column3, column4, column5, column6, column7, column8 };
}
