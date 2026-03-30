#pragma once
#include "Widget.g.h"

namespace winrt::TestCppWinRT::implementation
{
    struct Widget : WidgetT<Widget>
    {
        Widget() = default;
        Widget(hstring const& name);

        // Properties
        hstring Name() const;
        void Name(hstring const& value);
        TestCppWinRT::Priority Level() const;
        void Level(TestCppWinRT::Priority value);
        bool IsActive() const;
        void IsActive(bool value);
        int32_t Count() const;
        void Count(int32_t value);

        // Methods
        TestCppWinRT::Point GetPosition() const;
        void MoveTo(TestCppWinRT::Point const& pos);
        TestCppWinRT::Color GetColor() const;
        double ComputeDistance(TestCppWinRT::Point const& from, TestCppWinRT::Point const& to) const;
        bool TryParse(hstring const& input, int32_t& value) const;

        // Statics
        static TestCppWinRT::Widget CreateDefault();
        static hstring GetVersion();

        // Delegates
        void Subscribe(TestCppWinRT::StatusHandler const& handler);

        // Events
        winrt::event_token NameChanged(Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring> const& handler);
        void NameChanged(winrt::event_token token);

    private:
        hstring m_name;
        TestCppWinRT::Priority m_level{ TestCppWinRT::Priority::Medium };
        bool m_isActive{ false };
        int32_t m_count{ 0 };
        TestCppWinRT::Point m_position{};
        winrt::event<Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring>> m_nameChanged;
    };
}

namespace winrt::TestCppWinRT::factory_implementation
{
    struct Widget : WidgetT<Widget, implementation::Widget> {};
}
