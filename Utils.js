/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
} from 'react-native';

import Dimensions from 'Dimensions';
global.ScreenWidth = global.ScreenWidth || Dimensions.get('window').width;
global.ScreenHeight = global.ScreenHeight || Dimensions.get('window').height;
global.curWidth = global.curWidth || Math.min(ScreenWidth, ScreenHeight);
global.scaleWidth = global.scaleWidth || curWidth / 400;
global.minUnit = global.minUnit || ScreenWidth / 100;
global.unitDisSt = global.unitDisSt || curWidth / 25;
global.unitDisMv = global.unitDisMv || curWidth / 15;

var Utils = {
  NormalizedPointsCount: 32,
  blnGesture: false,
  matched: [],
  Dis: function (x, y){//计算第三边长度
    return Math.sqrt(x * x + y * y);
  },
  DisP: function(p1, p2){//计算两点间距离
    return Utils.Dis(p1.x - p2.x, p1.y - p2.y);
  },
  Lerp: function(a, b, f){//通过f来取a，b之间的差值，f取0-1
    if (f <= 0) return a;
    if (f >= 1) return b;
    return a + (b - a) * f;
  },
  LerpP: function(a, b, f){//通过f来取a点，b点之间的差值，f取0-1
    if (f <= 0) return a;
    if (f >= 1) return b;
    return {
      'x': Utils.Lerp(a.x, b.x, f),
      'y': Utils.Lerp(a.y, b.y, f)
    };
  },
  PathLength: function(points){//计算点集总共的长度
    var d=0;
    for(var i=1;i<points.length;i++){
      d += Utils.DisP(points[i-1], points[i]);
    }
    return d;
  },
  Resample: function(points, normalizedPointsCount){//按照设点的数量标准化点集
    normalizedPointsCount = Math.max(3, normalizedPointsCount);
    var intervalLength = Utils.PathLength(points) / (normalizedPointsCount-1);
    var D = 0;
    var q = {x:0, y:0};
    var normalizedPoints = [];
    normalizedPoints.push(points[0]);
    var pointBuffer = [];
    pointBuffer = pointBuffer.concat(points);
    for(var i=1;i<pointBuffer.length;i++){
      var a = pointBuffer[i-1];
      var b = pointBuffer[i];
      var d = Utils.DisP(a, b);
      if ((D+d) > intervalLength){
        q = Utils.LerpP(a, b, (intervalLength - D) / d);
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
  },
  ResampleByLen: function(points, len){//按照len的长度来标准化点集，使每个点的距离都一样
    len = Math.max(2, len);
    var normalizedPointsCount = parseInt(Utils.PathLength(points) / len);
    if (normalizedPointsCount <= 0) {
      return null;
    }
    return Utils.Resample(points, normalizedPointsCount);
  },
  CountDistance: function(arg1, arg2){//两点距离
    return Math.round(Math.sqrt(Math.pow(arg1.x - arg2.x, 2) + Math.pow(arg1.y - arg2.y, 2)));
  },
  CompareGesture: function(line1, line2){//比较手势，需要循环比较
    Utils.blnGesture = true;
    var nLine1 = Utils.Normalize(line1);
    var nLine2 = Utils.Normalize(line2);
    var e = 0.5;
    var step = Math.floor(Math.pow(nLine1.length, 1-e));
    var min = Number.MAX_VALUE;
    for(var i=0;i<nLine1.length;i+=step){
      var d1 = Utils.CloundDistance(nLine1, nLine2, i);
      var d2 = Utils.CloundDistance(nLine2, nLine1, i);
      min = Math.min(min, d1, d2);
    }
    return min;
  },
  CloundDistance: function(points1, points2, startIndex){//以一个数组中的一个起点为标准开始循环比较
    var numPoints = points1.length;
    Utils.ResetMatched(numPoints);
    if (points1.length != points2.length){
      console.warn('CloundDistance points1 count != points2 count');
      return Number.MAX_VALUE;
    }
    var sum = 0;
    var i = startIndex;
    do{
      var index = -1;
      var minDistance = Number.MAX_VALUE;
      for(var j=0;j<numPoints;j++){
        if (!Utils.matched[j]){
          var distance = Utils.Dis(points1[i], points2[j]);
          if (distance < minDistance){
            minDistance = distance;
            index = j;
          }
        }
      }
      matched[index] = true;
      var weight = 1 - ((i - startIndex + points1.length)%points1.length)/points1.length;
      sum += weight * minDistance;
      i = (i+1)%points1.length;
    }while(i != startIndex);
    return sum;
  },
  ResetMatched: function(count){//设置临时匹配数组，用于是否已经比较完毕的标识
    Utils.matched = [];
    for(var i=0;i<count;i++){
      Utils.matched.push(false);
    }
  },
  CompareNormal: function(line1, line2){//普通线段比较相似度
    Utils.blnGesture = false;
    var nLine1 = Utils.Normalize(line1);
    var nLine2 = Utils.Normalize(line2);
    return Utils.CloundNormal(nLine1, nLine2);
  },
  CloundNormal: function(points1, points2){//在相同的数量下进行比较
    if (points1.length != points2.length){
      console.warn('CloundNormal points1 count != points2 count');
      return Number.MAX_VALUE;
    }
    var sum = 0;
    for(var i=0;i<points1.length;i++){
      var distance = Utils.DisP(points1[i], points2[i]);
      sum += distance;
    }
    return sum;
  },
  Normalize: function(points){//标准化处理
    return Utils.Apply(points, Utils.NormalizedPointsCount);
  },
  Apply: function(inputPoints, normalizedPointsCount){
    var normalizedPoints = Utils.Resample(inputPoints, normalizedPointsCount);
    if (Utils.blnGesture){
      Utils.Scale(normalizedPoints);
      Utils.TranslateToOrigin(normalizedPoints);
    }
    return normalizedPoints;
  },
  Scale: function(points){//缩放点数组到标准尺寸
    var min = {x: Number.MAX_VALUE, y: Number.MAX_VALUE};
    var max = {x: Number.MIN_VALUE, y: Number.MIN_VALUE};
    for(var i=0;i<points.length;i++){
      var p = points[i];
      min.x = Math.min(min.x, p.x);
      min.y = Math.min(min.y, p.y);
      max.x = Math.max(max.x, p.x);
      max.y = Math.max(max.y, p.y);
    }
    var size = Math.max(max.x - min.x, max.y - min.y);
    var invSize = 1 / size;
    for(var i=0;i<points.length;i++){
      var p = points[i];
      p = Utils.PMulV(Utils.PsubP(p, min), invSize);
      points[i] = p;
    }
  },
  TranslateToOrigin: function(points){//数组中每一个点到几何中心做偏移处理
    var c = Utils.Centriod(points);
    for(var i=0;i<points.length;i++){
      var p = points[i];
      p = Utils.PsubP(p - c);
      points[i] = p;
    }
  },
  Centriod: function(points){//获取几何中心
    var c = {x:0, y:0};
    for(var i=0;i<points.length;i++){
      c = Utils.PAddP(c, points[i]);
    }
    c = Utils.PDivV(c, points.length);
    return c;
  },
  ImageCenter: function(points){//获取外框中心
    var min = {x: Number.MAX_VALUE, y: Number.MAX_VALUE};
    var max = {x: Number.MIN_VALUE, y: Number.MIN_VALUE};
    for(var i=0;i<points.length;i++){
      var p = points[i];
      min.x = Math.min(min.x, p.x);
      min.y = Math.min(min.y, p.y);
      max.x = Math.max(max.x, p.x);
      max.y = Math.max(max.y, p.y);
    }
    return {
      x: (max.x + min.x) / 2,
      y: (max.y + min.y) / 2,
    };
  },
  PAddP: function(a, b){//两点相加
    return {x: a.x + b.x, y: a.y + b.y};
  },
  PsubP: function(a, b){//两点相减
    return {x: a.x - b.x, y: a.y - b.y};
  },
  PMulV: function(a, v){//点乘以数值
    return {x: a.x * v, y: a.y * v}; 
  },
  PDivV: function(a, v){//点除以数值
    if (v == 0){
      console.warn('Utils PDivV the v is zero!!!!');
      v = 1;
    }
    return {x: a.x / v, y: a.y / v};
  }
};

module.exports = Utils;