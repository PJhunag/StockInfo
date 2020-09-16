import React, { Component } from 'react';
import { withStyles } from "@material-ui/core/styles";
import { Alert, AlertTitle } from '@material-ui/lab';

var getInfos = require('./getStockInfo.js')

const styles = theme => ({
  input: {
    display: 'flex',
    padding: 0,
  },
  valueContainer: {
    display: 'flex',
    flex: 1,
    alignItems: 'center',
  },
  chip: {
    margin: `${theme.spacing.unit / 2}px ${theme.spacing.unit / 4}px`,
  },
  noOptionsMessage: {
    fontSize: 16,
    padding: `${theme.spacing.unit}px ${theme.spacing.unit * 2}px`,
  },
  singleValue: {
    fontSize: 16,
  },
  placeholder: {
    position: 'absolute',
    left: 2,
    fontSize: 16,
  },
  margin: {
    margin: theme.spacing.unit
  }
});

var stock_no;
var preChoice;
var preNo;

class Stock extends Component {
  constructor(props) {
    super(props)
    this.state = {
      no: this.props.no,
      name: this.props.name,
      ev_list: [],
    }

    Stock.getDerivedStateFromProps = Stock.getDerivedStateFromProps.bind(this);
  }

  //有異動時觸發
  static getDerivedStateFromProps(prevProps, prevState) {

    if (preChoice != prevProps.name||preNo != prevProps.no) {
      stock_no = prevProps.no;
      console.log("現在處理:"+stock_no);
      this.handleChange_show_table(prevProps.name);
    }
    preNo = prevProps.no;
    preChoice = prevProps.name;
    return null;
  }

  async handleChange_show_table(type) {

    //呈現相關資料
    //stock_no = this.props.no;
    var time = {};
    time = getInfos.getDate(this.state.list_type);

    //取得編號部分
    if (typeof (stock_no) === "undefined") {
      return;
    }

    var ls_tmp = stock_no;
    if (ls_tmp.indexOf("(", 0) > 0) {
      stock_no = ls_tmp.substr(0, ls_tmp.indexOf("(", 0));
    }

    switch (type) {
      case "getEvaluations":
        //刷新營收
        var list = await getInfos.getEvaluations(stock_no);

        var evList = [];
        var ev = { name: null, value: null, lv: null, score: null };

        //perfect -  success
        //good    -  info
        //normal  -  warning
        //worst   -  error

        //拆解指標

        //目前價格
        ev.value = list.price;
        ev.name = "目前價格(" + list.updateDate + ")";
        ev.lv = "info";
        ev.score = "--"
        evList.push(ev);

        //1. EPS成長率
        //LastEPS  //去年EPS 
        //FiveYearsAvgerageEPS  //五年平均EPS 
        //去年EPS是否大於五年平均 -5% -5%~0% 0%~5% +5%
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = Math.round(list.lastEPS / list.fiveYearsAvgerageEPS * 100 - 100, 2);
        ev.name = "去年對比過去五年EPS成長率";
        if (ev.value > 10) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value > 5) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value > 0) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        ev.value = Math.round(list.lastEPS / list.fiveYearsAvgerageEPS * 100 - 100, 2) + "%"; //補上%
        evList.push(ev);


        //2. 本益比
        //LastFourEPS  //近四季EPS
        //preEPS  //預測EPS
        //preEPSRate  //預測本益比
        //本益比 10以下 10-12 12-14 14以上
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = list.preEPSRate;
        ev.name = "本年度預測本益比";
        if (ev.value < 10) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value < 12) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value < 14) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        evList.push(ev);


        //3.股利成長率
        //LastMoneyDividend  //去年股利
        //LastStockDividend  //去年股息
        //public string? RevenueIncreaseRatio  //發放率
        //preDividend  //預測股利
        //預測股利/去年股利 80%以下 80-100% 100%-120% 120%+
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = Math.round(list.preDividend / list.lastMoneyDividend * 100, 2);
        ev.name = "本年度預測現金股利";
        if (ev.value > 120) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value > 110) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value > 100) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        ev.value = list.preDividend;
        evList.push(ev);

        //4. 歷史殖利率
        //FiveYearsAvgerageDividend  //五年平均殖利率
        //整體殖利率 5%以下 5-6% 6-7% 7%以上
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = list.fiveYearsAvgerageDividend;
        ev.name = "過去五年殖利率";
        if (ev.value > 7) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value > 6) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value > 5) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        ev.value = list.fiveYearsAvgerageDividend + "%";
        evList.push(ev);

        //5. 預測殖利率
        //preDividendRadio  //預測殖利率
        //整體殖利率 5%以下 5-6% 6-7% 7%以上
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = list.preDividendRadio;
        ev.name = "本年度預測殖利率";
        if (ev.value > 7) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value > 6) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value > 5) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        ev.value = list.preDividendRadio + "%";
        evList.push(ev);

        //6 合理價位
        //goodPrice  //合理價位
        //與現價比 120%以上 110%-100% 100%-90% 90%以下
        var ev = { name: null, value: null, lv: null, score: null };
        ev.value = Math.round(list.goodPrice / list.price * 100, 2);
        ev.name = "合理價位";
        if (ev.value > 120) {
          ev.lv = "success";
          ev.score = 100;
        }
        else if (ev.value > 110) {
          ev.lv = "info";
          ev.score = 75;
        }
        else if (ev.value > 100) {
          ev.lv = "warning";
          ev.score = 50;
        }
        else {
          ev.lv = "error";
          ev.score = 25;
        }
        ev.value = list.goodPrice; //還原合理價格
        evList.push(ev);

        var evShow = [];
        var idx = 0;
        var acgScore = 0;
        evList.map(function (ev) {
          if (ev.score != "--") {
            acgScore = acgScore + ev.score;
            idx += 1;
          }
          evShow.push(
            <div>
              <br />
              <Alert severity={ev.lv}>
                <AlertTitle>{ev.name}</AlertTitle>
                <strong>{ev.value}</strong>
              </Alert>
            </div>)
        })

        var ev = { name: null, value: null, lv: null, score: null };
        ev.score = Math.round(acgScore / idx, 2);
        ev.name = "綜合評比";
        if (ev.score > 90) {
          ev.lv = "success";
          ev.value = "極推薦";
        }
        else if (ev.score > 70) {
          ev.lv = "info";
          ev.value = "推薦";
        }
        else if (ev.score > 50) {
          ev.lv = "warning";
          ev.value = "普通";
        }
        else {
          ev.lv = "error";
          ev.value = "不推薦";
        }

        evShow.push(
          <div>
            <br />
            <Alert severity={ev.lv}>
              <AlertTitle>{ev.name}</AlertTitle>
              <strong>{ev.value}</strong>
            </Alert>
          </div>);

        this.setState(state => ({ ev_list: evShow }));

        break;

    }

    //刷新畫面
    this.forceUpdate();
  }

  render() {
    return (
      <div >
        {this.state.ev_list}
      </div>
    )
  }
}

export default withStyles(styles)(Stock);
