/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  StyleSheet,
  Text,
  View,
  PanResponder,
  TouchableOpacity,
  StatusBar,
  ART,
} from 'react-native';

const {
  Shape,
  Group,
  Transform,
  Surface,
  Path,
  Pattern,
  LinearGradient,
  RadialGradient,
  // Text,
  ClippingRectangle,
} = ART;

var data = require('./data/瞅.json');
import Utils from './DrawUtils';
import DrawWord from './DrawWord1';
import DrawTouch from './DrawTouch1';

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,

  touch_begin: 0,
  touch_move: 1,
  touch_ended: 2,
};

export default class DrawLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.drawWord = null;
    this.nowPos = 0;
    this.touchLastPoint = null;

    this.mousePosition = null;
    this.lastMousePostion = null;
    this.arrOrgPoint = [];
    this.arrUsedPoint = [];
    this.nowR = 10;
    this.blnCanDraw = false;
    this.showPoints = null;

    this.arrGesture = [];
    this.InitGesture();
    this.InitWordInfo();
    
    this.status = cv.status_norm;
    for(var i=0;i<data.character.length;i++){
      var points = data.character[i].points;
      for(var k=0;k<points.length;k++){
        points[k].x = points[k].x * scaleWidth;
        points[k].y = points[k].y * scaleWidth;
      }
    }
    this.wrongCount = 0;
    this.state={
      blnUpdate: false,
    };
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate,
    });
  }
  InitGesture(){
    var bezierDataStr = require('./data/Gesture.json');
    for(var i=0;i<bezierDataStr.length;i++){
      var strTemp = bezierDataStr[i];
      var strArr0 = strTemp.split('|');
      var listPoint = [];
      if (strArr0.length > 0){
        var strArr1 = strArr0[0].split('_');
        for(var p=0;p<strArr1.length;p++){
          var strArr2 = strArr1[p].split(',');
          listPoint.push({
            x: parseFloat(strArr2[0]),
            y: parseFloat(strArr2[1]),
          });
        }
      }
      this.arrGesture.push(listPoint);
    }
  }
  InitWordInfo(){
    var strArr0 = require('./data/DataInfo.json');
    var strArr1 = require('./data/DrawInfo.json');
    var strArr2 = require('./data/OrderInfo.json');
    this.arrDataInfo = [];
    for(var i=0;i<strArr0.length;i++){
      var wi = {};
      var arr = strArr0[i].split('\t');
      wi.strHZ = arr[0];
      wi.strUnic = arr[1];
      wi.strPinyin = arr[2];
      wi.strHSK = arr[3];
      wi.strEnglish = arr[5].replace(';', '\n');
      wi.iCC = parseInt(arr[6]);
      wi.arrBJ = [];
      wi.arrBJUnic = [];
      wi.arrBJYI = [];
      wi.arrBJYIN = [];
      wi.iBJSL = 0;
      for(var m=7;m<arr.length;){
        wi.arrBJ[wi.iBJSL] = arr[i];
        wi.arrBJUnic[wi.iBJSL] = arr[i+1];
        wi.arrBJYI[wi.iBJSL] = parseInt(arr[i+2]);
        wi.arrBJYIN[wi.iBJSL] = parseInt(arr[i+3]);
        wi.iBJSL++;
        i+=4;
      }
    }
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
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      return false;
    }
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      return false;
    }
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.ResetDrawPoint();
      this.AddUsePoint(this.mousePosition, cv.touch_begin);
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      var s = Utils.DisP(this.mousePosition, this.lastMousePostion);
      if (s >= 1){
        this.AddUsePoint(this.mousePosition, cv.touch_move);
      }
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    this.mousePosition = {
      x: e.nativeEvent.locationX,
      y: e.nativeEvent.locationY
    };
    this.AddUsePoint(this.mousePosition, cv.touch_ended);
  }
  ResetDrawPoint(){
    this.arrOrgPoint = [];
    this.arrUsedPoint = [];
    this.nowR = 5;
    this.blnCanDraw = false;
    this.showPoints = null;
  }
  AddUsePoint(pos, kind){
    if (kind == cv.touch_begin){
      this.lastMousePostion = this.mousePosition;
      this.arrOrgPoint.push(pos);
      this.AddSinglePoint(pos, this.nowR);
      this.blnCanDraw = true;
    }else if (this.blnCanDraw){
      this.arrOrgPoint.push(pos);
      if (this.arrOrgPoint.length > 2){
        var listTemp = [];
        listTemp.push(this.arrOrgPoint[this.arrOrgPoint.length - 3]);
        listTemp.push(this.arrOrgPoint[this.arrOrgPoint.length - 2]);
        listTemp.push(this.arrOrgPoint[this.arrOrgPoint.length - 1]);
        listTemp = this.BSpline2Smooth1(listTemp, false);
        for(var i=0;i<listTemp.length;i++){
          this.AddSinglePoint(listTemp[i], this.nowR);
        }
        if (this.arrUsedPoint.length > 500){
          this.blnCanDraw = false;
        }
        // var count = Utils.DisP(this.lastMousePostion, pos);
        // if (count > 1){
        //   for(var i=0;i<count;i++){
        //     var p = Utils.LerpP(this.lastMousePostion, pos, (i+1)/count);
        //     this.AddSinglePoint(p, this.nowR);
        //   }
        // }
      }else{
        var count = Utils.DisP(this.lastMousePostion, pos);
        if (count > 1){
          var c = Math.ceil(count);
          for(var i=0; i < c; i++){
            if (i == c - 1){
              this.AddSinglePoint(pos, this.nowR);
            }else{
              var p = Utils.LerpP(this.lastMousePostion, pos, (i + 1) / c);
              this.AddSinglePoint(p, this.nowR);
            }
          }
        }else {
          this.AddSinglePoint(pos, this.nowR);
        }
      }
      if (kind == cv.touch_ended){
        this.CompareBihua();
      }
    }
    this.lastMousePostion = this.mousePosition;
    this.drawTouch && this.drawTouch.setPoints(this.showPoints);
  }
  AddSinglePoint(pos, r){
    this.arrUsedPoint.push(pos);
    d = new Path();
    for(var i=0;i<this.arrUsedPoint.length;i++){
      var p = this.arrUsedPoint[i];
      if (i==0){
        d.moveTo(p.x, p.y);
      }else{
        d.lineTo(p.x, p.y);
      }
    }
    this.showPoints = d;
  }
  BSpline2Smooth1(list, blnSet){
    var aList = [];
    aList = aList.concat(list);
    if (blnSet){
      aList.unshift(list[0]);
      aList.push(list[list.length - 1]);
    }
    var tList = [];
    var loc1 = 1;
    var start = {}, end = {};
    while(loc1 < aList.length - 1){
      start = aList[loc1 - 1];
      end = aList[loc1 + 1];
      tList.push(Utils.LerpP(aList[loc1-1], aList[loc1], 0.5));
      this.BSpline2Smooth2(tList, start, aList[loc1], end);
      tList.push(Utils.LerpP(aList[loc1], aList[loc1+1], 0.5));
      ++loc1;
    }
    var rl = Utils.ResampleByLen(tList, 2);
    if (rl != null){
      return rl
    }else{
      return tList;
    }
  }
  BSpline2Smooth2(list, arg1, arg2, arg3){
    var locx = [];
    var locy = [];
    locx.push((arg1.x + arg2.x) * 0.5);
    locx.push(arg2.x - arg1.x);
    locx.push((arg1.x - 2*arg2.x + arg3.x) * 0.5);
    locy.push((arg1.y + arg2.y) * 0.5);
    locy.push(arg2.y - arg1.y);
    locy.push((arg1.y - 2*arg2.y + arg3.y) * 0.5);
    var loc6 = parseInt(Utils.CountDistance(arg1, arg3));
    var loc7 = 0;
    var loc8 = 0;
    while(loc7 < loc6){
      loc8 = loc7 / loc6;
      var loc5 = {
        x: locx[0] + loc8 * (locx[1] + locx[2] * loc8),
        y: locy[0] + loc8 * (locy[1] + locy[2] * loc8)
      };
      list.push(loc5);
      loc7++;
    }
  }
  CompareBihua(){
    if (this.arrOrgPoint.length > 2){
      var showNum = 0;
      for(var i=0;i<data.character.length;i++){
        var ch = data.character[i];
        if (ch.isShow){
          showNum++;
          continue;
        }else{
          // var cValue = Utils.CompareGestrue(this.arrOrgPoint, )
        }
      }
    }
  }
  autoUpdate(){
    
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord style={styles.upView} 
          ref={(r)=>{this.drawWord = r}} 
          data={data}
        />
        <DrawTouch ref={(r)=>{this.drawTouch = r}} 
          data={this.showPoints}
          strokeColor={'rgb(0,0,255)'}
          strokeWidth={this.nowR}
        />
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    width: ScreenWidth,
    backgroundColor: '#F5FCFF'
  },
  mouseView:{
    position: 'absolute',
    left: 0,
    top: 0,
    width: ScreenWidth,
    height: ScreenHeight,
    backgroundColor: 'transparent'
  },
  upView:{
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downView: {
    width: ScreenWidth,
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: minUnit * 5,
  },
  autoWriteBtn:{
    marginBottom: minUnit * 5,
    marginLeft: minUnit * 5,
  },
  autoWriteText:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  },
  restartBtn:{
    marginBottom: minUnit * 5,
    marginRight: minUnit * 5,
  },
  restartText:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  }
});
