import * as React from "react";
import * as Models from "../Models/";
import * as interact from "interactjs";

interface ITimeSheetEditorSlotProps {
  slot: Models.ISlot;
  onSlotUpdate: Function;
  editSlot: (s: Models.ISlot) => void;
}

interface ITimeSheetEditorSlotState {}

export default class TimeSheetEditorSlot extends React.Component<
  ITimeSheetEditorSlotProps,
  ITimeSheetEditorSlotState
> {
  interactable: interact.Interactable | null = null;

  refs: {
    timeText: HTMLSpanElement;
    timeBar: HTMLSpanElement;
  };

  render() {
    var start = this.props.slot.StartMinutes,
      startPixels = (start * 1152) / 1440,
      len = this.props.slot.DurationMins,
      lenPixels = (len * 1152) / 1440,
      timeString = this.getTimestring(start, len);
    var barStyle = {
      marginLeft: startPixels + "px",
      width: lenPixels + "px",
      zIndex: 999
    };
    var txtStyle = {
      marginLeft: lenPixels + "px"
    };
    var classes = "timesheet-box";
    switch (this.props.slot.SlotType) {
      case 0:
        classes += " timesheet-box-sessions";
        break;
      case 1:
        classes += " timesheet-box-general";
        break;
    }
    return (
      <li>
        <span
          className={classes}
          data-id={this.props.slot.SlotId}
          data-oldstart={this.props.slot.StartMinutes}
          data-oldlength={this.props.slot.DurationMins}
          data-start={this.props.slot.StartMinutes}
          data-scale="48"
          data-length={this.props.slot.DurationMins}
          style={barStyle}
          title={this.props.slot.Title}
          onDoubleClick={e => this.doubleClicked()}
          ref="timeBar"
        >
          <strong>{this.props.slot.DayNr}</strong>{" "}
          <strong>{this.props.slot.LocationName}</strong>{" "}
          {this.props.slot.Title}
        </span>
        <span className="timesheet-time" style={txtStyle} ref="timeText">
          {timeString}
        </span>
      </li>
    );
  }

  componentDidMount() {
    var that = this;
    this.interactable = interact(this.refs.timeBar);
    this.interactable
      .draggable({
        inertia: false,
        restrict: {
          restriction: "parent",
          endOnly: true
        },
        autoScroll: false,
        onmove(event) {
          var target = event.target,
            x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx,
            hour = parseFloat(target.getAttribute("data-scale")),
            start = parseInt(target.getAttribute("data-oldstart")),
            scale = hour / 12,
            roundX = Math.round(x / scale) * scale,
            newMins = start + (60 * roundX) / hour,
            textSpan = target.nextElementSibling;
          target.style.webkitTransform = target.style.transform =
            "translate(" + roundX + "px, 0px)";
          target.setAttribute("data-x", x);
          target.setAttribute("data-start", newMins);
          textSpan.style.transform = "translate(" + roundX + "px, 0px)";
          textSpan.innerHTML = that.getTimestring(
            newMins,
            parseInt(target.getAttribute("data-length"))
          );
        },
        onend: that.updateSlot.bind(that)
      })
      .resizable({
        // preserveAspectRatio: false,
        edges: {
          left: false,
          right: true,
          bottom: false,
          top: false
        },
        onmove(event) {
          var target = event.target,
            dragLen = (event as any).rect.width,
            hour = parseFloat(target.getAttribute("data-scale")),
            scale = hour / 12,
            roundLen = Math.round(dragLen / scale) * scale,
            newMins = (60 * roundLen) / hour,
            textSpan = target.nextElementSibling;

          target.setAttribute("data-length", newMins);
          target.style.width = roundLen + "px";
          textSpan.innerHTML = that.getTimestring(
            parseInt(target.getAttribute("data-start")),
            newMins
          );
        },
        onend: that.updateSlot.bind(that)
      });
  }

  componentWillUnmount() {
    // this.interactable.unset();
    this.interactable = null;
  }

  getTimestring(start, len) {
    var timeString = (start % 60).toString();
    if (timeString.length < 2) {
      timeString = "0" + timeString;
    }
    timeString = Math.floor(start / 60).toString() + ":" + timeString + " ";
    var minsDuration = (len % 60).toString();
    if (minsDuration.length < 2) {
      minsDuration = "0" + minsDuration;
    }
    timeString += Math.floor(len / 60).toString() + ":" + minsDuration;
    return timeString;
  }

  updateSlot(event) {
    var timeBar = this.refs.timeBar,
      timeText = this.refs.timeText,
      slot = this.props.slot;
    slot.DurationMins = parseInt(timeBar.getAttribute("data-length") || "0");
    slot.NewStartMinutes = parseInt(timeBar.getAttribute("data-start") || "0");
    this.props.onSlotUpdate(slot, () => {
      timeBar.style.webkitTransform = timeBar.style.transform = null;
      timeText.style.transform = null;
      var len = this.props.slot.DurationMins,
        lenPixels = (len * 1152) / 1440;
      timeBar.style.width = lenPixels + "px";
    });
    var target = event.target,
      textSpan = target.nextElementSibling;
    target.style.webkitTransform = target.style.transform =
      "translate(0px, 0px)";
    textSpan.style.transform = "translate(0px, 0px)";
    return false;
  }

  doubleClicked() {
    this.props.editSlot(this.props.slot);
  }
}
