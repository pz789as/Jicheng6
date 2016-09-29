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

import Utils from './Utils';
import DrawWord from './DrawWord1';
import DrawTouch from './DrawTouch1';

let showTipTime = 80;

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,

  touch_begin: 0,
  touch_move: 1,
  touch_ended: 2,
};
let cd = {
  '不': require('./data/不.json'),
  '人': require('./data/人.json'),
  '八': require('./data/八.json'),
  '刀': require('./data/刀.json'),
  '分': require('./data/分.json'),
  '口': require('./data/口.json'),
  '可': require('./data/可.json'),
  '吞': require('./data/吞.json'),
  '告': require('./data/告.json'),
  '咽': require('./data/咽.json'),
  '哀': require('./data/哀.json'),
  '器': require('./data/器.json'),
  '天': require('./data/天.json'),
  '径': require('./data/径.json'),
  '扣': require('./data/扣.json'),
  '扩': require('./data/扩.json'),
  '拥': require('./data/拥.json'),
  '永': require('./data/永.json'),
  '江': require('./data/江.json'),
  '河': require('./data/河.json'),
  '波': require('./data/波.json'),
  '潘': require('./data/潘.json'),
  '目': require('./data/目.json'),
  '盲': require('./data/盲.json'),
  '睁': require('./data/睁.json'),
  '睦': require('./data/睦.json'),
  '瞅': require('./data/瞅.json'),
  '禾': require('./data/禾.json'),
  '秋': require('./data/秋.json'),
  '穴': require('./data/穴.json'),
  '轻': require('./data/轻.json'),
  '问': require('./data/问.json'),
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
    this.selWord = 0;
    this.status = cv.status_norm;
    this.wrongCount = 0;
    this.state={
      blnUpdate: false,
    };
    this.blnTips = false;
    this.tipsTime = 0;
    this.tipsText = '';
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate,
    });
  }
  InitGesture(){
    var bezierDataStr = require('./data/Gesture.json');
    for(var i=0;i<bezierDataStr.data.length;i++){
      var strTemp = bezierDataStr.data[i];
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
    for(var i=0;i<strArr0.data.length;i++){
      var wi = {};
      var arr = strArr0.data[i].split('\t');
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
        wi.arrBJ[wi.iBJSL] = arr[m];
        wi.arrBJUnic[wi.iBJSL] = arr[m+1];
        wi.arrBJYI[wi.iBJSL] = parseInt(arr[m+2]);
        wi.arrBJYIN[wi.iBJSL] = parseInt(arr[m+3]);
        wi.iBJSL++;
        m+=4;
      }

      var data = cd[wi.strHZ];
      if (data == null){
        console.log(wi.strHZ + '的json不存在');
        wi.character = null;
      }else{
        wi.character = data.character;
        for(var m=0;m<wi.character.length;m++){
          var min = {x: Number.MAX_VALUE, y: Number.MAX_VALUE};
          var max = {x: Number.MIN_VALUE, y: Number.MIN_VALUE};
          var points = wi.character[m].points;
          for(var k=0;k<points.length;k++){
            points[k].x = points[k].x * scaleWidth;
            points[k].y = points[k].y * scaleWidth;

            min.x = Math.min(min.x, points[k].x);
            min.y = Math.min(min.y, points[k].y);
            max.x = Math.max(max.x, points[k].x);
            max.y = Math.max(max.y, points[k].y);
          }
          wi.character[m].center = {
            x: (min.x + max.x)/2, 
            y: (min.y + max.y)/2
          };
          wi.character[m].isShow = false;
        }
      }

      arr = strArr2.data[i].split('\t');
      wi.arrOrder = [];
      for(var m=0;m<arr.length;m++){
        wi.arrOrder.push(parseInt(arr[m]));
      }

      this.arrDataInfo.push(wi);
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
      var blnSet = false;
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
        blnSet = true;
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
          blnSet = true;
        }else {
          // this.AddSinglePoint(pos, this.nowR);
          blnSet = false;
        }
      }
    }
    if (kind == cv.touch_ended){
      this.CompareBihua();
      this.ResetDrawPoint();
    }
    if (blnSet){
      this.lastMousePostion = this.mousePosition;
    }
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
      var wi = this.arrDataInfo[this.selWord];
      var showNum = 0;
      for(var i=0;i<wi.character.length;i++){
        var ch = wi.character[i];
        if (ch.isShow){
          showNum++;
          continue;
        }else{
          // Utils.NormalizedPointsCount = 64;
          // var cValue = Utils.CompareGesture(this.arrOrgPoint, this.arrGesture[wi.arrOrder[i]]);
          var cValue = Utils.CompareGesture(ch.dashPoints, this.arrOrgPoint);
          var cAngle = Utils.SumAngle(this.arrOrgPoint, true);
          var cAvgAngle = cAngle / this.arrOrgPoint.length;
          console.log('相似度：' + cValue, '原始点数量：' + ch.orgPoints.length);
          console.log('书写角度和：' + cAngle, '书写角度均值：' + cAvgAngle);
          console.log('原始角度和：' + ch.dashAngle, '原始角度均值：' + ch.dashAvgAngle);
          cAngle = Math.abs(cAngle - ch.dashAngle);
          // var blnIn = Utils.PointInPolygon(
          //   Utils.PSubP(this.arrOrgPoint[0],{x:relativeX, y: relativeY}), 
          //   ch.bspArr
          // );
          // console.log(blnIn);
          if (cValue <= 2 && cAngle <= 135){
            var writeAngle = Math.atan2(
              this.arrOrgPoint[this.arrOrgPoint.length-1].y - this.arrOrgPoint[0].y,
              this.arrOrgPoint[this.arrOrgPoint.length-1].x - this.arrOrgPoint[0].x
            ) * 180 / Math.PI;
            var baseAngle = Math.atan2(
              ch.orgPoints[ch.orgPoints.length-1].y - ch.orgPoints[0].y,
              ch.orgPoints[ch.orgPoints.length-1].x - ch.orgPoints[0].x
            ) * 180 / Math.PI;
            if (Math.abs(writeAngle - baseAngle) > 45){
              console.log('写反啦!', cValue);
              this.setTips('写反啦！');
              this.AddWrongCount();
            }else{
              showNum++;
              this.drawWord && this.drawWord.SetAnimation(i, this.arrOrgPoint);
              this.wrongCount = 0;
              console.log('匹配完毕', cValue);
            }
            break;
          }else{
            console.log('写的不对', cValue);
            this.AddWrongCount();
            break;
          }
        }
      }
      if (showNum == wi.character.length){
        console.log('书写完毕');
        this.setTips('写完啦！');
      }
    }
  }
  AddWrongCount(){
    this.wrongCount++;
    if (this.wrongCount == 3){
      this.drawWord && this.drawWord.setStrokeBlink();
      this.wrongCount = 0;
    }
  }
  setTips(text){
    this.tipsText = text;
    this.blnTips = true;
    this.tipsTime = showTipTime;
  }
  setDrawNext(){
    this.drawWord.setEndDraw();
    this.drawWord.drawIdx++;
    this.nowPos = 0;
    this.drawWord.setBeginDraw();
    this.wrongCount = 0;
  }
  onAutoWrite(){
    if (this.status == cv.status_norm){
      this.wrongCount = 0;
      this.status = cv.status_auto;
      this.nowPos = 0;
      if (this.drawWord){
        this.drawWord.setRestart();
      }
      this.setUpdate();
    }else if (this.status == cv.status_auto){
      this.status = cv.status_pause;
      this.setUpdate();
    }else if (this.status == cv.status_pause){
      this.status = cv.status_auto;
      this.setUpdate();
    }
  }
  autoUpdate(){
    var bln = false;
    if (this.status == cv.status_auto){
      if (this.drawWord){
        var idx = this.drawWord.drawIdx;
        if (idx>=0){
          var points = this.arrDataInfo[this.selWord].character[idx].orgPoints;
          if (this.nowPos >= points.length + 10){
            if (this.drawWord.drawIdx < this.arrDataInfo[this.selWord].character.length - 1){
              this.setDrawNext();
            }else{
              this.status = cv.status_norm;
              bln = true;
            }
          }else{
            this.nowPos += 0.25;
            this.drawWord.DrawingPecent(this.nowPos / points.length);
          }
        }
      }
    }
    if (this.blnTips){
      this.tipsTime--;
      if (this.tipsTime < 0){
        this.blnTips = false;
      }
      bln = true;
    }
    if (bln){
      this.setUpdate();
    }
  }
  autoWriteStop(){
    this.status = cv.status_norm;
    this.setUpdate();
  }
  onRestart(){
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      this.autoWriteStop();
    }
    if (this.drawWord){
      this.nowPos = 0;
      this.wrongCount = 0;
      this.drawWord.setRestart();
    }
  }
  onLast(){
    if (this.drawWord){
      if (this.selWord > 0){
        this.selWord--;
        if (this.arrDataInfo[this.selWord].character == null){
          this.onLast();
        }else{
          this.drawWord.initWord(this.arrDataInfo[this.selWord]);
          this.onRestart();
        }
      }
    }
  }
  onNext(){
    if (this.drawWord){
      if (this.selWord < this.arrDataInfo.length - 1){
        this.selWord++;
        if (this.arrDataInfo[this.selWord].character == null){
          this.onNext();
        }else{
          this.drawWord.initWord(this.arrDataInfo[this.selWord]);
          this.onRestart();
        }
      }
    }
  }
  onChangeView(){
    this.props.onPress();
  }
  getAutoText(){
    if (this.status == cv.status_auto){
      return '暂停书写';
    }else if (this.status == cv.status_norm){
      return '自动书写';
    }else if (this.status == cv.status_pause){
      return '继续书写';
    }
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord ref={(r)=>{this.drawWord = r}}
          style={styles.upView} 
          data={this.arrDataInfo[this.selWord]}
        />
        <DrawTouch ref={(r)=>{this.drawTouch = r}} 
          data={this.showPoints}
          strokeColor={'rgb(0,0,255)'}
          strokeWidth={this.nowR}
        />
        {this.DrawBottom()}
        <View style={styles.downChange}>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.onAutoWrite.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              {this.getAutoText()}
            </Text>
          </TouchableOpacity>
        </View>
        {this.DrawTips()}
      </View>
    );
  }
  DrawBottom(){
    return (
      <View style={styles.downView}>
        <TouchableOpacity style={styles.buttonStyle} onPress={this.onLast.bind(this)}>
          <Text style={styles.buttonTextStyle}>
            上一个
          </Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.buttonStyle} onPress={this.onRestart.bind(this)}>
          <Text style={styles.buttonTextStyle}>
            重新开始
          </Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.buttonStyle} onPress={this.onNext.bind(this)}>
          <Text style={styles.buttonTextStyle}>
            下一个
          </Text>
        </TouchableOpacity>
      </View>
    );
  }
  DrawTips(){
    if (this.tipsTime > 0){
      var opacity = 0.5;
      var t = showTipTime / 2;
      if (this.tipsTime >= t){
        opacity = 0.5 + 0.3 * (showTipTime-this.tipsTime)/t; 
      }else{
        opacity = 0.8 * this.tipsTime / t; 
      }
      return (
        <View style={[
          styles.tipsStyle, 
          {opacity: opacity}]}>
          <Text style={styles.tipsText}>
            {this.tipsText}
          </Text>
        </View>
      )
    }else{
      return null;
    }
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
    position: 'absolute',
    left: relativeX,
    top: relativeY,
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downChange:{
    position: 'absolute',
    left: 0,
    bottom: 10,
    width: ScreenWidth,
    height: minUnit * 10,
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  downView: {
    position: 'absolute',
    left: 0,
    bottom: 10 + minUnit * 10,
    width: ScreenWidth,
    height: minUnit * 10,
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  buttonStyle:{
    
  },
  buttonTextStyle:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  },
  tipsStyle:{
    position: 'absolute',
    left: ScreenWidth*3/8,
    top: ScreenHeight/2-minUnit*3,
    width: ScreenWidth/4,
    height: minUnit*6,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgb(0,0,0)',
    opacity: 0.5
  },
  tipsText:{
    color: 'red',
    fontSize: minUnit*4,
    textAlign: 'center',
  },
});
