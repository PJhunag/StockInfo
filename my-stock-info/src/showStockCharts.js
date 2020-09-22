//react
import React, { Component } from 'react';
import ReactEcharts from 'echarts-for-react';
import { emphasize } from '@material-ui/core/styles/colorManipulator';

//material-ui
import { withStyles } from "@material-ui/core/styles";
import purple from '@material-ui/core/colors/purple';
import Grid from '@material-ui/core/Grid';
import FormControl from '@material-ui/core/FormControl';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import RadioGroup from '@material-ui/core/RadioGroup';
import Radio from '@material-ui/core/Radio';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogTitle from '@material-ui/core/DialogTitle';
import Button from '@material-ui/core/Button';

//先準備相關資訊
var getInfos = require('./getStockInfo.js')
var getFavotie = require('./favorite.js')

//Module variable
var line_chart_list = {
  xAxis: {
    type: 'category',
    data: ['default']
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 500,
  },
  series: [{
    data: [0],
    type: 'line',
    smooth: true
  }]
};

var stock_no = "None"; //股票代碼
var stock_desc = "None"; //股票說明

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
  chipFocused: {
    backgroundColor: emphasize(
      theme.palette.type === 'light' ? theme.palette.grey[300] : theme.palette.grey[700],
      0.08,
    ),
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
  },
  search: {
    color: theme.palette.getContrastText(purple[200]),
    backgroundColor: purple[200],
    "&:hover": {
      backgroundColor: purple[400]
    },
    width: '90%',
    marginTop: '5%',
    marginLeft: '5%',
    marginright: '5%'
  },

});

class Stock extends Component {
  constructor(props) {
    super(props)
    this.state = {
      stock_no: this.props.no,
      list_type: "1_month",
      open: true,
      open2: false,
      open3: false,
      line_chart_list: line_chart_list,
      msg_open: false, //跳窗提示
      msg: "", //跳窗訊息
      favorited: false, //是否添加至我的最愛
      show_StockSelect: true, //顯示時間區間選項
    }

    this.handleChange_show_chart = this.handleChange_show_chart.bind(this);
    this.handleChange_type = this.handleChange_type.bind(this); //日期區間(type)

    //有外部預帶值, 帶出對應股票
    console.log("this.props.no:" + this.props.no);
    if (this.props.no != "") {
      stock_no = this.props.no;
      stock_desc = this.props.stock_desc;
      var chkfvrt = getFavotie.checkFavorite(stock_no);
      this.state.favorited = chkfvrt; //檢核是否已加入我的最愛
      this.handleChange_show_chart(this.props.name);
    }
  }

  //改變區間
  handleChange_type = key => (event, value) => {
    this.state.list_type = value;
    //this.setState(state => ({ list_type: value }));
    this.handleChange_show_chart(this.props.name);
  };

  componentWillReceiveProps(nextProps) {//componentWillReceiveProps方法中第一个参数代表即将传入的新的Props
    /*     console.log("刷新2!")
        console.log("this.props.no:" + this.props.no)
        console.log("nextProps.no:" + nextProps.no)
        console.log("this.props.name:" + this.props.name)
        console.log("nextProps.name:" + nextProps.name) */
    if (this.props.name !== nextProps.name || this.props.no !== stock_no) {
      //狀態有異動時觸發刷新
      this.handleChange_show_chart(nextProps.name);
    }
  }

  async handleChange_show_chart(type) {

    //呈現相關資料
    stock_no = this.props.no;
    console.log("no:" + this.props.no);
    console.log("type:" + this.state.list_type);

    var time = {};
    time = getInfos.getDate(this.state.list_type);

    //抓取當天日期
    var s_time = time.start; //起始時間
    var e_time = time.end; //截止時間

    //取得編號部分
    if (typeof (stock_no) === "undefined") {

      return;
    }

    var ls_tmp = stock_no;
    if (ls_tmp.indexOf("(", 0) > 0) {
      stock_no = ls_tmp.substr(0, ls_tmp.indexOf("(", 0));
    }
    var new_list;
    console.log("this.props.no:" + this.props.no);
    console.log("this.props.name:" + this.props.name);
    switch (type) {
      case 'getStockPrices':
        //取得價格資訊
        new_list = await getInfos.getStockPrices(stock_no, s_time, e_time, this.state.list_type);
        line_chart_list = JSON.parse(new_list);
        //顯示時間區間選擇
        this.setState(state => ({ show_StockSelect: true }));
        break;
      case "getEPS":
        //刷新EPS
        new_list = await getInfos.getEPS(stock_no);
        console.log("new_list:"+new_list)
        line_chart_list = new_list;
        //顯示時間區間選擇
        this.setState(state => ({ show_StockSelect: false }));
        break;
      default:
        //取得價格資訊
        new_list = await getInfos.getStockPrices(stock_no, s_time, e_time, this.state.list_type);
        line_chart_list = JSON.parse(new_list);
        break;
    }

    //刷新畫面
    this.forceUpdate();
  }

  //點選確認後關閉
  msg_close = () => {
    this.setState(state => ({ msg_open: false }));
  };

  render() {
    return (
      <div>
        {this.state.show_StockSelect &&
          <Grid item xs={12}>
            <FormControl component="fieldset">
              <RadioGroup row name="avatar" aria-label="avatar" value={this.state.list_type} onChange={this.handleChange_type('list_type')}>
                <FormControlLabel value="1_month" control={<Radio />} label="最近一個月" />
                <FormControlLabel value="3_month" control={<Radio />} label="最近三個月" />
                <FormControlLabel value="6_mounth" control={<Radio />} label="最近半年" />
                <FormControlLabel value="1_year" control={<Radio />} label="最近一年" />
                <FormControlLabel value="3_year" control={<Radio />} label="最近三年" />
                <FormControlLabel value="all_year" control={<Radio />} label="全部資料" />
              </RadioGroup>
            </FormControl>
          </Grid>}
        <p />
        <div className='child'>
          <ReactEcharts
            option={line_chart_list}
            style={{ height: '400px', width: '95%' }}
            className='react_for_echarts' />
        </div>
        <Dialog open={this.state.msg_open} aria-labelledby="form-dialog-title" >
          <DialogTitle id="form-dialog-title">{this.state.msg}</DialogTitle>
          <DialogActions>
            <Button onClick={this.msg_close} color="primary">確認</Button>
          </DialogActions>
        </Dialog>
      </div>
    )
  }
}
export default withStyles(styles)(Stock);