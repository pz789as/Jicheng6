/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
  PanResponder,
  TouchableOpacity,
  StatusBar,
} from 'react-native';

import Dimensions from 'Dimensions';
let ScreenWidth = Dimensions.get('window').width;
global.ScreenWidth = global.ScreenWidth || ScreenWidth;
let ScreenHeight = Dimensions.get('window').height;
global.ScreenHeight = global.ScreenHeight || ScreenHeight;
console.log(ScreenWidth, ScreenHeight);

let curWidth = Math.min(ScreenWidth, ScreenHeight);
global.curWidth = global.curWidth || curWidth;
let scaleWidth = curWidth / 400;
global.scaleWidth = global.scaleWidth || scaleWidth;
let minUnit = ScreenWidth / 100;
global.minUnit = global.minUnit || minUnit;
let unitDisSt = curWidth / 25;
let unitDisMv = curWidth / 15;

import XzwWord from './XzwWord';

var data = require('./data/瞅.json');

let Dis = function(x, y){
  return Math.sqrt(x * x + y * y);
};
global.Dis = global.Dis || Dis;
let DisP = function(p1, p2){
  return Dis(p1.x - p2.x, p1.y - p2.y);
};
global.DisP = global.DisP || DisP;
let Lerp = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return a + (b - a) * f;
}
global.Lerp = global.Lerp || Lerp;
let LerpP = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return {
    'x': Lerp(a.x, b.x, f),
    'y': Lerp(a.y, b.y, f)
  };
}
global.LerpP = global.LerpP || LerpP;

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,
};

export default class XzwLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.touchLastPoint = null;
    this.state={
      blnUpdate: false,
    };
    this.initData();
  }
  initData(){
    this.listGesture = [];
    var gesData = require('./data/Gesture.json');
    for(var i=0;i<gesData.data.length;i++){
      var str = gesData.data[i];
      var arr0 = str.split('|');
      var listPoint = [];
      var arr1 = arr0[0].split('_');
      for(var k=0;k<arr1.length;k++){
        var arr2 = arr1[k].split(',');
        listPoint.push({
          x: parseFloat(arr2[0]),
          y: parseFloat(arr2[1]),
          z: parseFloat(arr2[2])
        });
      }
      this.listGesture.push(listPoint);
    }

    var dataInfo = require('./data/DataInfo.json');
    var drawInfo = require('./data/DrawInfo.json');
    var orderInfo = require('./data/OrderInfo.json');
    this.arrDataInfo = [];//保存所有的WordInfo结构
    //根据三个json，初始化dataInfo数据，里面是WordInfo结构
    // WordInfo = {
    //   strHZ: "",   //汉字字符
    //   strUnic: "",   //unicode的码
    //   strPinyin: "", //拼音
    //   trHSK: "",  //hsk等级
    //   strGJ: "", //国际教育等级
    //   iCC: 0, //层次类型
    //   iBJSL: 0,   //部件数量
    //   arrBJ: [],  //部件字符
    //   arrBJUinc: [],    //部件字体中所用unicode的码
    //   arrBJYI: [],  //部件表义
    //   arrBJYIN: [], //部件表音
    //   strEnglish: "", //英文
    //   drawInfo: "", //绘制信息
    //   arrOrder: [], //笔画顺序
    //   arrBihua: [], //笔画信息
    //   arrBSCenter: [] //每个笔画的中心点
    // }
    for(var i=0;i<dataInfo.data.length;i++){
      var wi = {};
      
      //初始化dataInfo数据
      var arr = dataInfo.data[i].split('\t');
      wi.strHZ = arr[0];
      wi.strUnic = arr[1];
      wi.strPinyin = arr[2];
      wi.strHSK = arr[3];
      wi.strGJ = arr[4];
      wi.strEnglish = arr[5].replace(';', '\n');
      wi.iBJSL = 0;
      wi.iCC = parseInt(arr[6]);
      wi.arrBJ = [];
      wi.arrBJUnic = [];
      wi.arrBJYI = [];
      wi.arrBJYIN = [];
      for(var k=7;k<arr.length;){
        wi.arrBJ.push(arr[k]);
        wi.arrBJUnic.push(arr[k+1]);
        wi.arrBJYI.push(parseInt(arr[i+2]));
        wi.arrBJYIN.push(parseInt(arr[i+3]));
        wi.iBJSL++;
        k+=4;
      }

      //初始化drawInfo数据
      wi.drawInfo = drawInfo.data[i];
      wi.arrBihua = [];
      wi.arrBSCenter = [];
      var arr0 = wi.drawInfo.split('|');
      for(var k=0;k<arr0.length;k++){
        if (k<arr0.length-1){
          var arr1 = arr0[k].split('_');
          var arr2 = arr1[0].split(',');
          var tempBihua = {};
          tempBihua.center = {
            x: parseFloat(arr2[0]),
            y: parseFloat(arr2[1]),
            z: 0,
          };
          tempBihua.orgList = [];
          tempBihua.orgList.push({
            x: parseFloat(arr2[2]),
            y: parseFloat(arr2[3]),
            z: 0,
          });
          tempBihua.orgList.push({
            x: parseFloat(arr2[4]),
            y: parseFloat(arr2[5]),
            z: 0,
          });
          tempBihua.isShow = false;
          tempBihua.bushou = parseInt(arr2[6]);
          if (arr2.length>7){
            tempBihua.picName = arr2[7] + '/' + k.toString();
          }

          arr2 = arr1[1].split(',');
          tempBihua.basePoint = [];
          for(var j=0;j<arr2.length;j++){
            tempBihua.basePoint.push({
              x: parseFloat(arr2[j*2+0]),
              y: parseFloat(arr2[j*2+1]),
              z: 0,
            });
          }
          wi.arrBihua.push(tempBihua);
        }else{
          var arr1 = arr0[k].split(',');
          for(var j=0;j<arr1.length/2;j++){
            wi.arrBSCenter.push({
              x: parseFloat(arr1[j*2+0]),
              y: parseFloat(arr1[j*2+1]),
              z: 0,
            });
          }
        }
      }

      // 初始化oder数据
      var arrOrder = orderInfo.data[i].split('\t');
      wi.arrOrder = [];
      for(var k=0;k<arrOrder.length;k++){
        wi.arrOrder.push(parseInt(arrOrder[k]));
      }

      this.arrDataInfo.push(wi);
    }
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate,
    });
  }
  componentWillMount() {
    this._panResponder = PanResponder.create({
      onStartShouldSetPanResponder: this.onStartShouldSetPanResponder.bind(this),
      onMoveShouldSetPanResponder: this.onMoveShouldSetPanResponder.bind(this),
      onPanResponderGrant: this.onPanResponderGrant.bind(this),
      onPanResponderMove: this.onPanResponderMove.bind(this),
      onPanResponderRelease: this.onPanResponderRelease.bind(this),
      onPanResponderTerminate: this.onPanResponderTerminate.bind(this),
    });
  }
  componentDidMount() {
    this._autoUpdate = setInterval(this.autoUpdate.bind(this), 1/60);
  }
  componentWillUnmount() {
    this._autoUpdate && clearInterval(this._autoUpdate);
  }
  
  onStartShouldSetPanResponder(e, g){
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      var tp = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.touchLastPoint = tp;
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      var tp = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.touchLastPoint = tp;
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    console.log('touch end');
  }
  autoUpdate(){
    
  }
  lastWord(){
    console.log('last');
  }
  nextWord(){
    console.log('next');
  }
  rewriteWord(){
    console.log('rewrite');
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <XzwWord ref={(r)=>{this.xzwWord = r}} style={styles.upView} data={this.arrDataInfo[0]}/>
        <View style={styles.downView}>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.lastWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              上一个
            </Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.rewriteWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              重写
            </Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.nextWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              下一个
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    backgroundColor: '#F5FCFF'
  },
  upView:{
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downView: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: minUnit * 5,
    width: ScreenWidth,
  },
  buttonStyle:{
    marginBottom: minUnit * 5,
  },
  buttonTextStyle:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  }
});
