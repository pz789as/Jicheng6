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

import DrawWord from './DrawWord1';

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
var PathLength = function(points){
  var d=0;
  for(var i=1;i<points.length;i++){
    d += DisP(points[i-1], points[i]);
  }
  return d;
};
global.PathLength = global.PathLength || PathLength;
var Resample = function(points, normalizedPointsCount){
  normalizedPointsCount = Math.max(3, normalizedPointsCount);
  var intervalLength = PathLength(points) / (normalizedPointsCount-1);
  var D = 0;
  var q = {x:0, y:0};
  var normalizedPoints = [];
  normalizedPoints.push(points[0]);
  var pointBuffer = [];
  pointBuffer = pointBuffer.concat(points);
  for(var i=1;i<pointBuffer.length;i++){
    var a = pointBuffer[i-1];
    var b = pointBuffer[i];
    var d = DisP(a, b);
    if ((D+d) > intervalLength){
      q = LerpP(a, b, (intervalLength - D) / d);
      normalizedPoints.push(q);
      pointBuffer.splice(i, 0, q);
      D = 0;
    }else{
      D += d;
    }
  }
  if (normalizedPoints.length == normalizedPointsCount - 1){
    normalizedPoints.push(pointBuffer[pointBuffer.length - 1]);
  }
  return normalizedPoints;
};
global.Resample = global.Resample || Resample;
var ResampleByLen = function(points, len){
  len = Math.max(2, len);
  var normalizedPointsCount = parseInt(PathLength(points) / len);
  if (normalizedPointsCount <= 0) {
    return null;
  }
  return Resample(points, normalizedPointsCount);
};
global.ResampleByLen = global.ResampleByLen || ResampleByLen;

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,

  touch_begin: 0,
  touch_move: 1,
  touch_ended: 2,

  scaleMin: 0.7,
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
    this.arrShowPoint = [];
    this.nowR = 5;
    this.blnCanDraw = false;
    
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
      var s = DisP(this.mousePosition, this.lastMousePostion);
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
    this.arrShowPoint = [];
    this.nowR = 5;
    this.blnCanDraw = false;
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
        if (this.arrUsedPoint.length > 600){
          this.blnCanDraw = false;
        }
      }else{
        var count = DisP(this.lastMousePostion, pos);
        if (count > 1){
          var c = Math.ceil(count);
          for(var i=0; i < c; i++){
            if (i == c - 1){
              this.AddSinglePoint(pos, this.nowR);
            }else{
              var p = LerpP(this.lastMousePostion, pos, (i + 1) / c);
              this.AddSinglePoint(p, this.nowR);
            }
          }
        }else {
          this.AddSinglePoint(pos, this.nowR);
        }
      }
    }
    this.lastMousePostion = this.mousePosition;
    this.setUpdate();
  }
  AddSinglePoint(pos, scale){
    pos.r = scale;
    var d = new Path();
    d.moveTo(pos.x, pos.y - pos.r);
    d.arc(0, pos.r*2, pos.r).arc(0, -pos.r*2, pos.r).close();
    pos.d = d;
    pos.color = 'rgb(0,0,255)';
    pos.time = 0;
    this.arrUsedPoint.push(pos);
    this.arrShowPoint.push(
      <Shape d={pos.d} fill={pos.color}/>
    );
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
      tList.push(LerpP(aList[loc1-1], aList[loc1], 0.5));
      this.BSpline2Smooth2(tList, start, aList[loc1], end);
      tList.push(LerpP(aList[loc1], aList[loc1+1], 0.5));
      ++loc1;
    }
    var rl = ResampleByLen(tList, 2);
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
    var loc6 = parseInt(this.CountDistance(arg1, arg3));
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
  CountDistance(arg1, arg2){
    return Math.round(Math.sqrt(Math.pow(arg1.x - arg2.x, 2) + Math.pow(arg1.y - arg2.y, 2)));
  }
  SetEndPoint(bln){

  }
  autoUpdate(){
    
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord style={styles.upView} ref={(r)=>{this.drawWord = r}} data={data}/>
        <View style={styles.mouseView}>
          <Surface ref={'lineView'} width={ScreenWidth} height={ScreenHeight}>
            {this.arrShowPoint}
          </Surface>
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
