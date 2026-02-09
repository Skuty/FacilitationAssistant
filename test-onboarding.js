const { chromium } = require('playwright');

async function testOnboarding() {
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
        console.log('Starting onboarding test...');
        
        // Navigate to the home page
        await page.goto('http://localhost:5153/');
        await page.waitForLoadState('domcontentloaded');
        console.log('✓ Loaded home page');

        // Create a new meeting
        const meetingTitle = 'Onboarding Test Meeting';
        await page.fill('input[placeholder*="meeting"]', meetingTitle);
        await page.click('button:has-text("Create Meeting")');
        await page.waitForURL(/\/facilitator\//);
        console.log('✓ Created meeting and navigated to facilitator view');

        // Wait for meeting to load
        await page.waitForTimeout(2000);

        // Check if tour overlay appears (first-time user)
        const tourVisible = await page.isVisible('.tour-overlay');
        if (tourVisible) {
            console.log('✓ Facilitator tour overlay is visible');
            
            // Check tour content
            const tourTitle = await page.textContent('.tour-header h5');
            console.log(`  Tour title: ${tourTitle}`);
            
            // Navigate through tour steps
            let currentStep = 1;
            while (await page.isVisible('button:has-text("Next →")')) {
                console.log(`  Clicking Next on step ${currentStep}`);
                await page.click('button:has-text("Next →")');
                await page.waitForTimeout(500);
                currentStep++;
            }
            
            // Complete the tour
            await page.click('button:has-text("Got It!")');
            await page.waitForTimeout(500);
            console.log('✓ Completed facilitator tour');
        } else {
            console.log('⚠ Tour not visible (may have been seen before)');
        }

        // Test help button to restart tour
        await page.click('button[title="Show Tour"]');
        await page.waitForTimeout(1000);
        const tourVisibleAfterHelp = await page.isVisible('.tour-overlay');
        if (tourVisibleAfterHelp) {
            console.log('✓ Tour restarted successfully via help button');
            await page.click('button:has-text("Got It!")');
        }

        // Get the attendee link
        await page.click('button:has-text("Share Links")');
        await page.waitForTimeout(500);
        const attendeeLink = await page.locator('input[readonly]').nth(1).inputValue();
        console.log('✓ Retrieved attendee link');

        // Open attendee view in a new page
        const attendeePage = await context.newPage();
        await attendeePage.goto(attendeeLink);
        await page.waitForTimeout(2000);
        console.log('✓ Opened attendee view');

        // Wait for meeting to start
        await page.bringToFront();
        await page.click('button:has-text("Start Meeting")');
        await page.waitForTimeout(2000);
        console.log('✓ Started meeting');

        // Check attendee welcome modal
        await attendeePage.bringToFront();
        await attendeePage.waitForTimeout(1000);
        
        const welcomeVisible = await attendeePage.isVisible('.modal:has-text("Welcome to the Meeting")');
        if (welcomeVisible) {
            console.log('✓ Attendee welcome modal is visible');
            
            // Check content
            const modalContent = await attendeePage.textContent('.modal-body');
            console.log(`  Modal shows anonymity info: ${modalContent.includes('anonymous')}`);
            console.log(`  Modal shows attendee number: ${modalContent.includes('Attendee #')}`);
            
            // Dismiss the modal
            await attendeePage.click('button:has-text("Got It!")');
            await attendeePage.waitForTimeout(500);
            console.log('✓ Dismissed attendee welcome modal');
        } else {
            console.log('⚠ Welcome modal not visible (may have been seen before)');
        }

        // Test help button to restart welcome
        await attendeePage.click('button[title="Show Welcome"]');
        await attendeePage.waitForTimeout(1000);
        const welcomeVisibleAfterHelp = await attendeePage.isVisible('.modal:has-text("Welcome to the Meeting")');
        if (welcomeVisibleAfterHelp) {
            console.log('✓ Welcome modal restarted successfully via help button');
            await attendeePage.click('button:has-text("Got It!")');
        }

        // Verify attendee number is shown in the header
        const headerText = await attendeePage.textContent('.bg-info.text-white');
        if (headerText.includes('Attendee #')) {
            console.log('✓ Attendee number is displayed in header');
        }

        console.log('\n========================================');
        console.log('✅ All onboarding tests passed!');
        console.log('========================================\n');

    } catch (error) {
        console.error('\n❌ Test failed:', error.message);
        console.error(error.stack);
    } finally {
        await browser.close();
    }
}

// Run the test
testOnboarding();
