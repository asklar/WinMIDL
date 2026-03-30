#pragma once
#include "Widget.g.h"

namespace winrt::TestCppWinRT::implementation
{
    struct Widget : WidgetT<Widget>
    {
        Widget() = default;
        Widget(hstring const& name);

        hstring Name() const;
        void Name(hstring const& value);
        TestCppWinRT::Priority Level() const;
        void Level(TestCppWinRT::Priority value);
        TestCppWinRT::Point GetPosition() const;
        void MoveTo(TestCppWinRT::Point const& pos);
        TestCppWinRT::Color GetColor() const;
        double ComputeDistance(TestCppWinRT::Point const& from, TestCppWinRT::Point const& to) const;
        static TestCppWinRT::Widget CreateDefault();
        static hstring GetVersion();
        void Subscribe(TestCppWinRT::StatusHandler const& handler);
        winrt::event_token NameChanged(Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring> const& handler);
        void NameChanged(winrt::event_token token);

    private:
        hstring m_name;
        TestCppWinRT::Priority m_level{ TestCppWinRT::Priority::Medium };
        TestCppWinRT::Point m_position{};
        winrt::event<Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring>> m_nameChanged;
    };
}

namespace winrt::TestCppWinRT::factory_implementation
{
    struct Widget : WidgetT<Widget, implementation::Widget> {};
}
