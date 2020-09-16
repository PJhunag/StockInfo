import React, { Component } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Paper from '@material-ui/core/Paper';
import { withStyles } from "@material-ui/core/styles";

var getInfos = require('./getStockInfo.js')
var title = ['col0', 'col1', 'col2', 'col3', 'col4', 'col5', 'col6', 'col7', 'col8']

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
var preNo;
var preChoice;

function createTable(column0, column1, column2, column3, column4, column5, column6, column7, column8) {
  return { column0, column1, column2, column3, column4, column5, column6, column7, column8 };
}

var tableList = [];

class Stock extends Component {
  constructor(props) {
    super(props)
    this.state = {
      stock_no: null,
      choice_name: null,
    }

    Stock.getDerivedStateFromProps = Stock.getDerivedStateFromProps.bind(this);
  }



  /*   componentWillReceiveProps(nextProps) {//componentWillReceiveProps方法中第一个参数代表即将传入的新的Props
           console.log("刷新表格!")
          console.log("this.props.no:" + this.props.no)
          console.log("nextProps.no:" + nextProps.no)
          console.log("this.props.name:" + this.props.name)
          console.log("nextProps.name:" + nextProps.name) 
      //if (this.props.name !== nextProps.name || this.props.no !== stock_no) {
      //狀態有異動時觸發刷新
      //console.log("刷新表格!!!")
      this.handleChange_show_table(nextProps.name);
      //}
    } */


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
    //console.log("this.props.no:" + this.props.no);
    //console.log("this.props.name:" + this.props.name);
    switch (type) {
      case "getOperatingIncomes":
        //刷新營收
        title = ['年度', '月份 ', '單月營收', '去年同月營收', '單月月增率', '單月年增率', '累計營收', '去年累計營收', '累積年增率'];
        tableList = await getInfos.getOperatingIncomes(stock_no);
        break;

      case "getDividend":
        //刷新股利
        title = ['發放年度', '除權日', '除息日', '除權息前股價', '股票股利', '現金股利', 'EPS', '配息率', '現金殖利率'];
        tableList = await getInfos.getDividend(stock_no);
        break;


      case "getProfit":
        //刷新損益
        title = ['年度', '季別 ', '營收	', '毛利	', '營業利益', '稅前淨利', '稅後淨利'];
        tableList = await getInfos.getProfit(stock_no);
        break;
      default:
        //取得價格資訊
        //new_list = await getInfos.getStockPrices(stock_no, s_time, e_time, this.state.list_type);
        //line_chart_list = JSON.parse(new_list);
        break;
    }

    //刷新畫面
    this.forceUpdate();
  }

  render() {
    const { classes } = this.props;
    return (
      <div className='parent'>
        <TableContainer component={Paper}>
          <Table className={classes.table} size="small" aria-label="a dense table">
            <TableHead>
              <TableRow>
                <TableCell align="center">{title[0]}</TableCell>
                <TableCell align="center">{title[1]}</TableCell>
                <TableCell align="center">{title[2]}</TableCell>
                <TableCell align="center">{title[3]}</TableCell>
                <TableCell align="center">{title[4]}</TableCell>
                <TableCell align="center">{title[5]}</TableCell>
                <TableCell align="center">{title[6]}</TableCell>
                <TableCell align="center">{title[7]}</TableCell>
                <TableCell align="center">{title[8]}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tableList.map((row) => (
                <TableRow key={row.name}>
                  <TableCell align="center">{row.column0}</TableCell>
                  <TableCell align="center">{row.column1}</TableCell>
                  <TableCell align="center">{row.column2}</TableCell>
                  <TableCell align="center">{row.column3}</TableCell>
                  <TableCell align="center">{row.column4}</TableCell>
                  <TableCell align="center">{row.column5}</TableCell>
                  <TableCell align="center">{row.column6}</TableCell>
                  <TableCell align="center">{row.column7}</TableCell>
                  <TableCell align="center">{row.column8}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </div>
    )
  }
}

export default withStyles(styles)(Stock);
