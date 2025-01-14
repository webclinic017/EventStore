using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using EventStore.Common.Configuration;
using EventStore.Common.Utils;
using EventStore.Core.Telemetry;
using Xunit;

namespace EventStore.Core.XUnit.Tests.Telemetry;

public class SystemMetricsTests : IDisposable {
	private readonly TestMeterListener<float> _floatListener;
	private readonly TestMeterListener<double> _doubleListener;
	private readonly TestMeterListener<long> _longListener;
	private readonly FakeClock _clock = new();
	private readonly SystemMetrics _sut;

	public SystemMetricsTests() {
		var meter = new Meter($"{typeof(ProcessMetricsTests)}");
		_floatListener = new TestMeterListener<float>(meter);
		_doubleListener = new TestMeterListener<double>(meter);
		_longListener = new TestMeterListener<long>(meter);

		var config = new Dictionary<TelemetryConfiguration.SystemTracker, bool>();

		foreach (var value in Enum.GetValues<TelemetryConfiguration.SystemTracker>()) {
			config[value] = true;
		}
		_sut = new SystemMetrics(meter, TimeSpan.FromSeconds(42), config);
		_sut.CreateLoadAverageMetric("eventstore-sys-load-avg", new() {
			{ TelemetryConfiguration.SystemTracker.LoadAverage1m, "1m" },
			{ TelemetryConfiguration.SystemTracker.LoadAverage5m, "5m" },
			{ TelemetryConfiguration.SystemTracker.LoadAverage15m, "15m" },
		});

		_sut.CreateCpuMetric("eventstore-sys-cpu");

		_sut.CreateMemoryMetric("eventstore-sys-mem", new() {
			{ TelemetryConfiguration.SystemTracker.FreeMem, "free" },
			{ TelemetryConfiguration.SystemTracker.TotalMem, "total" },
		});

		_sut.CreateDiskMetric("eventstore-sys-disk", ".", new() {
			{ TelemetryConfiguration.SystemTracker.DriveTotalBytes, "total" },
			{ TelemetryConfiguration.SystemTracker.DriveUsedBytes, "used" },
		});

		_floatListener.Observe();
		_doubleListener.Observe();
		_longListener.Observe();
	}

	public void Dispose() {
		_floatListener.Dispose();
		_doubleListener.Dispose();
		_longListener.Dispose();
	}

	[Fact]
	public void can_collect_sys_load_avg() {
		if (!OS.IsUnix)
			return;

		Assert.Collection(
			_doubleListener.RetrieveMeasurements("eventstore-sys-load-avg"),
			m => {
				Assert.True(m.Value > 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("period", tag.Key);
						Assert.Equal("1m", tag.Value);
					});
			},
			m => {
				Assert.True(m.Value > 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("period", tag.Key);
						Assert.Equal("5m", tag.Value);
					});
			},
			m => {
				Assert.True(m.Value > 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("period", tag.Key);
						Assert.Equal("15m", tag.Value);
					});
			});
	}

	[Fact]
	public void can_collect_sys_cpu() {
		if (OS.IsUnix)
			return;

		Assert.Collection(
			_floatListener.RetrieveMeasurements("eventstore-sys-cpu"),
			m => {
				Assert.True(m.Value >= 0);
				Assert.Empty(m.Tags);
			});
	}

	[Fact]
	public void can_collect_sys_mem() {
		Assert.Collection(
			_longListener.RetrieveMeasurements("eventstore-sys-mem-bytes"),
			m => {
				Assert.True(m.Value > 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("kind", tag.Key);
						Assert.Equal("free", tag.Value);
					});
			},
			m => {
				Assert.True(m.Value > 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("kind", tag.Key);
						Assert.Equal("total", tag.Value);
					});
			});
	}

	[Fact]
	public void can_collect_sys_disk() {
		Assert.Collection(
			_longListener.RetrieveMeasurements("eventstore-sys-disk-bytes"),
			m => {
				Assert.True(m.Value >= 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("kind", tag.Key);
						Assert.Equal("used", tag.Value);
					},
					tag => {
						Assert.Equal("disk", tag.Key);
						Assert.NotNull(tag.Value);
					});
			},
			m => {
				Assert.True(m.Value >= 0);
				Assert.Collection(
					m.Tags,
					tag => {
						Assert.Equal("kind", tag.Key);
						Assert.Equal("total", tag.Value);
					},
					tag => {
						Assert.Equal("disk", tag.Key);
						Assert.NotNull(tag.Value);
					});
			});
	}
}
